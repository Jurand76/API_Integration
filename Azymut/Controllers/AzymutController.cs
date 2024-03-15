using Microsoft.AspNetCore.Mvc;
using FormatSoapAPI;

using System.Xml.Serialization;
using Azymut.Models;
using Azymut.Helpers;
using DataAccessLibrary.Models.Azymut;
using DataAccessLibrary.Data.Azymut;
using System.Data.SqlClient;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using CsvHelper;
using System.Globalization;
using DataAccessLibrary.Models.Shoper;
using DataAccessLibrary.Data.Shoper;
using Azymut.Models.Azymut;
using Azymut.Models.Shoper;
using DataAccessLibrary.Models.Hangfire;
using System.Net.Mail;


namespace Azymut.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    internal class AzymutController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IAzymutDataService _azymutDataService;
        private readonly IShoperDataService _shoperDataService;
        private readonly IShoperService _shoperService;
        private readonly IAzymutService _azymutService;
        private readonly string synchronizationGuid;

        public iFormatSoapAPIClient clientAzymut { get; set; }

        public AzymutController(IHttpClientFactory httpClient, IAzymutDataService azymutDataService, IAzymutService azymutService, IShoperDataService shoperDataService, IShoperService shoperService)
        {
            _httpClient = httpClient;
            _azymutDataService = azymutDataService;
            _azymutService = azymutService;
            _shoperDataService = shoperDataService;
            _shoperService = shoperService;

            synchronizationGuid = "C30E78AE-C5BA-4C22-8487-F7C21F3AEAA2";
            clientAzymut = new iFormatSoapAPIClient();
        }


        // Filling Azymut SQL database - Products table with new books and prices
        [HttpGet("GetNewAzymutProductsAndSaveToSQL")]
        public async Task<IActionResult> GetNewAzymutProductsAndSaveToSQL()
        {
            return Unauthorized();

            // Get Azymut api login and pass
            AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

            // Get Azymut SynchronizationId
            AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);

            // get session Id string
            string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

            // get categories
            getProducts categories = new getProducts();
            categories.sessionID = sessionID;
            var categoriesResponse = await clientAzymut.getProductsAsync(categories);

            // get existing books codes in database (ShoperIntegration table) to avoid adding the same books
            List<string> existingCodesInSQL = await _azymutService.GetExistingCodesFromSQLAsync();

            // new products counter
            int newProductsAdded = 0;

            // iterate through products in each category and save to SQL database
            HttpClient soapClient = _httpClient.CreateClient();

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

                    // check if product code exists in database, if not - add new item
                    if (!existingCodesInSQL.Contains(azymutProduct.Id))
                    {
                        // create and save new book to SQL
                        Book newBookToAdd = _azymutService.CreateBookForSQL(categoryName, azymutProduct);
                        await _azymutService.SaveNewAzymutBookAsync(newBookToAdd);

                        // create price of product for specified Synchronization Id with specified margin
                        Price newPriceToAdd = _azymutService.CreatePriceForSQL(apiAzymutUserData.PriceMargin, azymutProduct);
                        newPriceToAdd.SynchronizationId = azymutSynchronizationProperties.Id;
                        await _azymutService.SaveNewAzymutPriceAsync(newPriceToAdd);

                        // add new code to existing codes list
                        existingCodesInSQL.Add(azymutProduct.Id);
                        newProductsAdded += 1;
                    }
                }
            }

            clientAzymut.Close();
            return Ok($"Added new {newProductsAdded} books");
        }

        [HttpGet("UpdateAzymutProductsWithNewTypeId")]
        public async Task<IActionResult> UpdateAzymutProductsWithNewTypeId()
        {
            //return Unauthorized();

            try
            {
                // Get Azymut api login and pass
                AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

                // Get Azymut SynchronizationId
                AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);

                // get session Id string
                string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

                // get categories
                getProducts categories = new getProducts();
                categories.sessionID = sessionID;
                var categoriesResponse = await clientAzymut.getProductsAsync(categories);

                // get existing books codes in database (ShoperIntegration table) to avoid adding the same books
                List<string> existingCodesInSQL = await _azymutService.GetExistingCodesFromSQLAsync();

                // updated products counter
                int updatedProducts = 0;

                // iterate through products in each category and save to SQL database
                HttpClient soapClient = _httpClient.CreateClient();

                for (int catNr = 0; catNr < categoriesResponse.getProductsResponse.getProductsResult.result.productsA.Count(); catNr++)
                {

                    Console.WriteLine($"Przetwarzanie kategorii {catNr}");

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

                        // check if product code exists in database
                        if (existingCodesInSQL.Contains(azymutProduct.Id))
                        {
                            // create and save new book to SQL
                            BookTypeId bookToUpdate = new BookTypeId();
                            bookToUpdate.Code = azymutProduct.Id;
                            string mediaType = azymutProduct.Typ.Nazwa.ToLower();

                            if (azymutProduct.Issues.IssueList.Count != 0)
                            {
                                bookToUpdate.TypeId = azymutProduct.Issues.IssueList[0].IssueAtrybuty.Format;
                            }
                            else
                            {
                                if (mediaType == "ebook" || mediaType == "eprasa")
                                {
                                    bookToUpdate.TypeId = "epub";
                                }
                                else
                                {
                                    bookToUpdate.TypeId = "mp3";
                                }
                            }

                            if (bookToUpdate.TypeId.IsNullOrEmpty())
                            {
                                if (mediaType == "ebook" || mediaType == "eprasa")
                                {
                                    bookToUpdate.TypeId = "epub";
                                }
                                else
                                {
                                    bookToUpdate.TypeId = "mp3";
                                }
                            }


                            await _azymutService.ChangeTypeIdAsync(bookToUpdate.Code, bookToUpdate.TypeId);
                            updatedProducts += 1;

                            if ((updatedProducts + 1) % 2000 == 0)
                            {
                                Console.WriteLine($"Updated {updatedProducts - 1} books");
                            }
                        }
                    }
                }

                clientAzymut.Close();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
            
            return Ok();
        }

        [HttpGet("CopyAzymutCategoriesToShoper")]
        public async Task<IActionResult> CopyAzymutCategoriesToShoper()
        {
            return Unauthorized();

            // Get Azymut api login and pass
            AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

            // Get Azymut SynchronizationId
            AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);

            // get session Id string
            string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

            // Get Shoper token
            ApiUserShoper apiShoperUserData = await _shoperDataService.GetShoperApiUserAsync(synchronizationGuid);
            string token = await _shoperService.GetTokenForApiAsync(apiShoperUserData.BaseUrl, apiShoperUserData.ApiUser, apiShoperUserData.ApiPassword);

            // Get main category number
            string mainCategoryString = apiShoperUserData.MainCategory.ToString();
            int mainCategory = 0;
            if (mainCategoryString.IsNullOrEmpty())
            {
                throw new Exception("Main category number not defined!");
            }
            else
            {
                mainCategory = Convert.ToInt16(mainCategoryString);
            }

            // get Azymut session
            getSession session = new getSession();
            session.username = apiAzymutUserData.ApiUser;
            session.password = apiAzymutUserData.ApiPassword;

            // get categories
            getProducts categories = new getProducts();
            categories.sessionID = sessionID;
            var categoriesResponse = await clientAzymut.getProductsAsync(categories);

            // create list of book's categories 
            List<string> categoriesList = new List<string>();
            for (int i = 0; i < categoriesResponse.getProductsResponse.getProductsResult.result.productsA.Count(); i++)
            {
                string categoryName = categoriesResponse.getProductsResponse.getProductsResult.result.productsA[i].title;
                categoriesList.Add(categoryName);
            }

            // save categories in Shoper
            int numberOfNewCategories = await _shoperService.SaveCategoriesToShoper(apiShoperUserData.BaseUrl, token, mainCategory, categoriesList);

            Console.WriteLine($"Added new {numberOfNewCategories} categories.");

            clientAzymut.Close();
            return Ok();
        }

        [HttpGet("GetOrdersHistory")]
        public async Task<IActionResult> GetOrdersHistory()
        {
            return Unauthorized();

            // Get Azymut api login and pass
            AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

            // Get Azymut SynchronizationId
            AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);
            int synchronizationId = azymutSynchronizationProperties.Id;

            // get session Id string
            string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

            // get list of orders to send - with status = 1 in Orders table
            List<OrderSQL> newOrders = await _azymutService.GetOrdersFromSQLAsync(synchronizationId, 1);

            getOrdersHistory newHistory = new getOrdersHistory();

            newHistory.sessionID = sessionID;
            newHistory.start = 0;
            newHistory.limit = 50;

            var ordersHistoryResponse = await clientAzymut.getOrdersHistoryAsync(newHistory);

            if (ordersHistoryResponse.getOrdersHistoryResponse.getOrdersHistoryResult.msg == "OK")
            {
                int ordersCount = ordersHistoryResponse.getOrdersHistoryResponse.getOrdersHistoryResult.result.orders.Count();
                string lastOrderId = "";
                if (ordersCount > 0)
                {
                    lastOrderId = ordersHistoryResponse.getOrdersHistoryResponse.getOrdersHistoryResult.result.orders[ordersCount - 1].orderID;
                }

                return Ok($"Orders history has been read. Found {ordersCount} orders. Last order id: {lastOrderId}");
            }

            return Ok("No orders history");
        }

        [HttpGet("GetOrderLink")]
        public async Task<IActionResult> GetOrderLink(string orderID)
        {
            return Unauthorized();

            // Get Azymut api login and pass
            AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

            // Get Azymut SynchronizationId
            AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);
            int synchronizationId = azymutSynchronizationProperties.Id;

            // get session Id string
            string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

            getOrderMultiformatInfo orderInfo = new getOrderMultiformatInfo();

            orderInfo.sessionID = sessionID;
            orderInfo.orderID = orderID;
            var orderInfoResponse = await clientAzymut.getOrderMultiformatInfoAsync(orderInfo);

            if (orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.msg == "OK")
            {
                return Ok($"Link: {orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.result.items[0].content[0].urls}");
            }

            return Ok($"Error API message: {orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.msg}");
        }

        [HttpGet("UpdateCategoriesShoperToThree")]
        public async Task<IActionResult> UpdateCategoriesShoperToThree()
        {
            return Unauthorized();

            // Get Shoper token
            ApiUserShoper apiShoperUserData = await _shoperDataService.GetShoperApiUserAsync(synchronizationGuid);
            string token = await _shoperService.GetTokenForApiAsync(apiShoperUserData.BaseUrl, apiShoperUserData.ApiUser, apiShoperUserData.ApiPassword);

            int result = await _shoperService.UpdateCategoriesInShoperAsync(apiShoperUserData.BaseUrl, token);
            return Ok();
        }
    }
}
