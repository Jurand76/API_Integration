using Microsoft.AspNetCore.Mvc;
using FormatSoapAPI;
using Azymut.Helpers;
using DataAccessLibrary.Models.Shoper;
using DataAccessLibrary.Data.Shoper;
using Microsoft.IdentityModel.Tokens;
using DataAccessLibrary.Data.Azymut;
using DataAccessLibrary.Models.Azymut;
using Azymut.Models;
using Azymut.Models.Shoper;
using System.Xml.Serialization;
using DataAccessLibrary.Models.Hangfire;
using System.Diagnostics;
using Azymut.Models.Azymut;
using DataAccessLibrary.Data.Hangfire;
using RequestHelpersLibrary;
using System.ComponentModel.DataAnnotations;


namespace AzymutShoper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzymutShoperProductsController : ControllerBase
    {
        private readonly IShoperDataService _shoperDataService;
        private readonly IShoperService _shoperService;
        private readonly IHttpClientFactory _httpClient;
        private readonly IAzymutDataService _azymutDataService;
        private readonly IAzymutService _azymutService;
        private readonly IHangfireDataService _hangfireDataService;
        private readonly IRequestValidator _validator;
        private readonly IRequestAliveConnectionKeeper _aliveConnectionKeeper;
        public iFormatSoapAPIClient clientAzymut { get; set; }


        public AzymutShoperProductsController(IHangfireDataService hangfireDataService, IAzymutService azymutService, IHttpClientFactory httpClient, IAzymutDataService azymutDataService, IShoperService shoperService, IShoperDataService shoperDataService, IRequestValidator validator, IRequestAliveConnectionKeeper aliveConnectionKeeper)
        {
            _shoperDataService = shoperDataService;
            _shoperService = shoperService;
            _httpClient = httpClient;
            _azymutDataService = azymutDataService;
            _azymutService = azymutService;
            _hangfireDataService = hangfireDataService;
            clientAzymut = new iFormatSoapAPIClient();
            _validator = validator;
            _aliveConnectionKeeper = aliveConnectionKeeper;
        }

        [HttpGet("Run")]
        public async Task<IActionResult> Run(string synchronizationGuid)
        {
            // Validate call
            if (_validator.ValidateRequest(Request.Headers["Authorization"]) == false)
            {
                //return Unauthorized();
            }

            // Since Tim can run longer than Azure's supported limit, spawning a connection keeper
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task.Run(() => _aliveConnectionKeeper.SpawnKeepConnectionAliveTask(this.Response, cancellationToken));

            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                // Get Azymut api login, pass and price margin for selected user 
                AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);
                decimal priceMargin = apiAzymutUserData.PriceMargin;

                // Get Azymut SynchronizationId
                AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);
                int synchronizationId = azymutSynchronizationProperties.Id;

                // get session Id string
                string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

                // get items synchronization history
                DatabaseSync lastSynchronization = await _azymutService.GetLastSynchronizationAsync();
                getItemsHistory getHistory = new getItemsHistory();
                DateTime dateTimeStartSynchro = DateTime.Now;
                getHistory.date = lastSynchronization.Date;
                getHistory.sessionID = sessionID;
                var historyResponse = await clientAzymut.getItemsHistoryAsync(getHistory);

                // finish procedure if there is no elements to update
                if (historyResponse.getItemsHistoryResponse.getItemsHistoryResult.changeLog.logs == null
                    || historyResponse.getItemsHistoryResponse.getItemsHistoryResult.changeLog.logs.Count() == 0)
                {
                    sw.Stop();
                    _aliveConnectionKeeper.CancelKeepAliveToken(cancellationToken);
                    return Ok($"Nothing to update from {lastSynchronization.Date}");
                }

                // get categories
                getProducts categories = new getProducts();
                categories.sessionID = sessionID;
                var categoriesResponse = await clientAzymut.getProductsAsync(categories);

                // get existing books codes in database (ShoperIntegration table) to avoid adding the same books
                List<string> existingCodesInSQL = await _azymutService.GetExistingCodesFromSQLAsync();

                // list of existing prices from SQL for selected synchronizationId, and list of prices to update, with Shoper Id 
                List<PriceWithShoperId> existingPrices = await _azymutService.GetExistingPricesFromSQLAsync(synchronizationId);
                List<PriceWithShoperId> pricesToUpdate = new List<PriceWithShoperId>();

                // counters
                int newProductsAddedtoSQL = 0;
                int newPricesCounter = 0;
                int checkedPricesCounter = 0;

                // iterate through products in each category and save to SQL database
                HttpClient soapClient = _httpClient.CreateClient();
                soapClient.Timeout=TimeSpan.FromMinutes(10);

                for (int catNr = 0; catNr < categoriesResponse.getProductsResponse.getProductsResult.result.productsA.Count(); catNr++)
                {
                    Console.WriteLine($"Przetwarzanie kategorii {catNr}");
                    string categoryName = categoriesResponse.getProductsResponse.getProductsResult.result.productsA[catNr].title;

                    // get products catalog url from Azymut API
                    var url = categoriesResponse.getProductsResponse.getProductsResult.result.productsA[catNr].url;

                    // get books from catalog
                    var httpResponse = await soapClient.GetAsync(url);
                    httpResponse.EnsureSuccessStatusCode();
                    var stream = await httpResponse.Content.ReadAsStreamAsync();
                    var serializer = new XmlSerializer(typeof(Kartoteki));
                    Kartoteki azymutProducts = new Kartoteki();
                    azymutProducts = (Kartoteki)serializer.Deserialize(stream);

                    for (int productNumber = 0; productNumber < azymutProducts.KartotekaList.Count; productNumber++)
                    {
                        // take one book from Azymut catalog
                        Kartoteka azymutProduct = azymutProducts.KartotekaList[productNumber];

                        // check if product code exists in database, if not exists - add new item
                        if (!existingCodesInSQL.Contains(azymutProduct.Id))
                        {
                            // create and save new book to SQL
                            Book newBookToAdd = _azymutService.CreateBookForSQL(categoryName, azymutProduct);
                            await _azymutService.SaveNewAzymutBookAsync(newBookToAdd);

                            // create price of product for specified Synchronization Id with specified margin
                            Price newPriceToAdd = _azymutService.CreatePriceForSQL(apiAzymutUserData.PriceMargin, azymutProduct);
                            newPriceToAdd.SynchronizationId = synchronizationId;
                            await _azymutService.SaveNewAzymutPriceAsync(newPriceToAdd);

                            // add new code to existing codes list
                            existingCodesInSQL.Add(azymutProduct.Id);
                            newProductsAddedtoSQL += 1;
                        }
                        else
                        {
                            // if product exists in database - check if its price was changed at Azymut
                            if (azymutProduct.Issues.IssueList.Count > 0)
                            {
                                // check price in Azymut and convert it to Shoper model
                                PriceWithShoperId azymutPrice = _azymutService.ConvertPriceToShoperModel(azymutProduct, priceMargin, synchronizationId);

                                // find price in Shoper prices table
                                PriceWithShoperId foundShoperPrice = existingPrices.Find(price => price.Code == azymutProduct.Id.ToString());
                                if (foundShoperPrice != null)
                                {
                                    // check if buying price has been changed - if yes - add it to upgrade list
                                    if (foundShoperPrice.PriceBuying != azymutPrice.PriceBuying)
                                    {
                                        azymutPrice.ShoperId = foundShoperPrice.ShoperId;
                                        pricesToUpdate.Add(azymutPrice);
                                        newPricesCounter += 1;
                                    }

                                    checkedPricesCounter += 1;
                                }
                            }
                        }
                    }
                }

                // Get Shoper token
                ApiUserShoper apiUserData = await _shoperDataService.GetShoperApiUserAsync(synchronizationGuid);
                string token = await _shoperService.GetTokenForApiAsync(apiUserData.BaseUrl, apiUserData.ApiUser, apiUserData.ApiPassword);

                // read parameters of Shoper Integration for customer
                Synchronization synchronizationProperties = await _hangfireDataService.GetSynchronizationPropertiesAsync(synchronizationGuid);
                int shoperSynchronizationId = synchronizationProperties.Id;
                Azymut.Models.Shoper.ShoperParameters shoperParameters = await _shoperService.GetShoperParametersAsync(shoperSynchronizationId);

                // Get main category number and main producer
                int mainProducer = shoperParameters.MainProducerId;

                int mainCategory = shoperParameters.MainCategoryId;
                if (mainCategory == 0)
                {
                    throw new Exception("Main category number not defined!");
                }

                // get shoper taxes list
                List<Tax> taxShoper = await _shoperService.GetTaxesIdAsync(apiUserData.BaseUrl, token);

                // get shoper categories tree to find proper categories numbers
                List<CategoriesTree> shoperCategories = await _shoperService.GetCategoriesTreeWithNamesShoperAsync(apiUserData.BaseUrl, token);

                // get list of books to add to Shoper
                List<NewBookSQL> newBooksToAdd = await _shoperService.GetNewBooksToAdd(synchronizationId);

                // send newBooks list to Shoper 
                int itemsAddedtoShoper = await _shoperService.SendProductsFromSQLToShoper(apiUserData.BaseUrl, token, shoperCategories, taxShoper, newBooksToAdd, mainCategory, mainProducer, synchronizationId);

                // update prices in Shoper and SQL
                int pricesUpdated = await _shoperService.UpdatePricesInShoperAndSQL(apiUserData.BaseUrl, token, pricesToUpdate);

                // update DatabaseSync table with new synchronization time
                DatabaseSync updateSyncData = new DatabaseSync()
                {
                    Date = dateTimeStartSynchro,
                    ItemsChanged = newProductsAddedtoSQL + pricesUpdated
                };
                await _azymutService.SaveSynchronizationDataAsync(updateSyncData);

                sw.Stop();

                clientAzymut.Close();

                _aliveConnectionKeeper.CancelKeepAliveToken(cancellationToken);

                // For long running processes we need to write to the response
                await this.Response.WriteAsync($"Time: {sw.ElapsedMilliseconds / 1000}(s). Added new {newProductsAddedtoSQL} items to SQL. Added new {itemsAddedtoShoper} books to Shoper. Checked {checkedPricesCounter} prices. {newPricesCounter} to update. {pricesUpdated} prices updated.");
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _aliveConnectionKeeper.CancelKeepAliveToken(cancellationToken);
                return BadRequest(ex.Message);
            }
        }
    }
}