using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Azymut.Models.Shoper;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using DataAccessLibrary.Databases;
using Azymut.Models;
using DataAccessLibrary.Data.Tim;
using System;
using Microsoft.IdentityModel.Tokens;
using Azymut.Models.Azymut;
using RequestHelpersLibrary;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace Azymut.Helpers
{
    public class ShoperService : IShoperService
    {

        private readonly ISqlData _db;
        private readonly string _azymutConnectionString;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShoperService(ISqlData db, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _azymutConnectionString = configuration.GetConnectionString("AzymutConnection");
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<NewBookSQL>> GetNewBooksToAdd(int synchronizationId)
        {
            var result = await _db.GetDataListAsync<NewBookSQL, dynamic>(_azymutConnectionString, "dbo.sp_GetProductsNotInShoperIntegration", new { @SynchronizationId = synchronizationId });
            return result;
        }

        public async Task<ShoperParameters> GetShoperParametersAsync(int synchronizationId)
        {
            var result = await _db.GetDataTypeAsync<ShoperParameters, dynamic>(_azymutConnectionString, "dbo.sp_GetShoperParameters", new { @synchronizationId = synchronizationId });
            return result;
        }

        public async Task AddNetItemToShoperIntegration(NewIntegrationItem newIntegrationItem)
        {
            await _db.SaveDataAsync(_azymutConnectionString, "dbo.sp_AddItemToShoperIntegration", newIntegrationItem);
        }

        public async Task UpdatePriceAtPricesTable(PriceWithShoperId priceToUpdate)
        {
            await _db.SaveDataAsync(_azymutConnectionString, "dbo.sp_UpdatePrice", priceToUpdate);
        }

        public async Task<string> GetTokenForApiAsync(string urlForApi, string login, string password)
        {
            // Set up the client
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", $"{login}:{password}".EncodeBase64());
            client.Timeout = TimeSpan.FromSeconds(300);

            var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                var response = await client.PostAsync(urlForApi + "auth", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error: " + response.StatusCode);
                }

                var result = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

                return tokenResponse?.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: " + ex.Message);
                return null;
            }
        }

        public async Task<List<OrderShoper>> GetShoperPaidOrdersAsync(string url, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            List<OrderShoper> ordersList = new List<OrderShoper>();
            int pageCounter = 0;
            int maxElements = 50;
            bool notLastPage = false;

            do
            {
                var response = await client.GetAsync(url + $"orders?limit={maxElements}&page={pageCounter}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching categories: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<OrdersApiResponse>(content);
                List<OrderShoper> temporaryList = new List<OrderShoper>();
                temporaryList = apiResponse.Orders;

                foreach (var order in temporaryList)
                {
                    if (order.IsPaid)
                    {
                        ordersList.Add(order);
                    }
                }

                if (temporaryList.Count == maxElements)
                {
                    notLastPage = true;
                    pageCounter += 1;
                }
                else
                {
                    notLastPage = false;
                }

                await Task.Delay(800);
            }
            while (notLastPage);
            return ordersList;
        }

        public async Task<List<OrderProduct>> GetShoperOrdersProductsAsync(string url, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            List<OrderProduct> ordersProductsList = new List<OrderProduct>();
            int pageCounter = 0;
            int maxElements = 50;
            bool notLastPage = false;

            do
            {
                var response = await client.GetAsync(url + $"order-products?limit={maxElements}&page={pageCounter}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching categories: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<OrdersProductsApiResponse>(content);
                List<OrderProduct> temporaryList = new List<OrderProduct>();
                temporaryList = apiResponse.OrdersProducts;
                                
                if (temporaryList.Count == maxElements)
                {
                    notLastPage = true;
                    pageCounter += 1;
                }
                else
                {
                    notLastPage = false;
                }

                ordersProductsList.AddRange(temporaryList);
                await Task.Delay(800);
            }
            while (notLastPage);
            return ordersProductsList;
        }

        public async Task<List<Category>> GetProductCategoriesAsync(string url, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            List<Category> finalList = new List<Category>();
            int pageCounter = 0;
            int maxElements = 50;
            bool notLastPage = false;

            do
            {
                var response = await client.GetAsync(url + $"categories?limit={maxElements}&page={pageCounter}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching categories: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<CategoriesApiResponse>(content);
                List<Category> temporaryList = new List<Category>();
                temporaryList = apiResponse.Categories;

                if (temporaryList.Count == maxElements)
                {
                    notLastPage = true;
                    pageCounter += 1;
                }
                else
                {
                    notLastPage = false;
                }
                finalList.AddRange(temporaryList);
            }
            while (notLastPage);
           
            return finalList;
        }

        public async Task<List<CategoriesTree>> GetCategoriesTreeWithNamesShoperAsync(string apiUrl, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            var categoryResponse = await client.GetAsync(apiUrl + $"categories-tree");
            var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
            var categoriesTree = JsonConvert.DeserializeObject<List<CategoriesTree>>(categoryContent);

            List<Category> categoryListNames = await GetProductCategoriesAsync(apiUrl, token);

            foreach (var category2 in categoriesTree)
            {
                foreach (var categoryChild in category2.Children)
                {
                    foreach (var category in categoryListNames)
                    {
                        if (categoryChild.ChildId == category.CategoryId)
                        {
                            categoryChild.Name = category.Translations["pl_PL"].Name;
                        }
                    }
                }

                foreach (var category in categoryListNames)
                {
                    if (category2.Id == category.CategoryId)
                    {
                        category2.Name = category.Translations["pl_PL"].Name;
                    }
                }
            }

            return categoriesTree;
        }

        public async Task<List<Tax>> GetTaxesIdAsync(string url, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            var response = await client.GetAsync(url + $"taxes");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching taxes: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<TaxApiResponse>(content);
            List<Tax> taxesList = apiResponse.TaxesId;
            
            return taxesList;
        }

        public bool VerifyEAN13(string barcode)
        {
            if (barcode.IsNullOrEmpty() || barcode.Length != 13 || !long.TryParse(barcode, out _))
            {
                return false; // Not 13-digits code
            }

            int sum = 0;
            for (int i = 0; i < barcode.Length - 1; i++)
            {
                int digit = barcode[i] - '0'; // Conversion char for int
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == barcode[12] - '0'; // Compare to last digit
        }

        public async Task<int> UpdateOrderStatusAsync(string apiUrl, string token, int orderId, int orderStatus)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            var newStatus = new
            {
                status_id = orderStatus
            };

            var newItemJSON = JsonConvert.SerializeObject(newStatus);
            var content = new StringContent(newItemJSON, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(apiUrl + $"orders/{orderId}", content);

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }
            var responseContent = await response.Content.ReadAsStringAsync();

            return Convert.ToInt32(responseContent);
        }

        public async Task<int> AddProductToShoper(string apiUrl, string token, ShoperNewProduct newProductToAdd)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            var newItemJSON = JsonConvert.SerializeObject(newProductToAdd);
            var content = new StringContent(newItemJSON, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl + "products", content);

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return Convert.ToInt32(responseContent);
        }

        public async Task<int> AddImageToProduct(string apiUrl, string token, int shoperId, string imageUrl)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            object newImage = new
            {
                product_id = shoperId,
                url = imageUrl
            };

            var newItemJSON = JsonConvert.SerializeObject(newImage);
            var content = new StringContent(newItemJSON, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl + "product-images", content);

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            return Convert.ToInt32(responseContent);
        }

        public async Task<int> SendProductsFromSQLToShoper(string apiBaseUrl, string token, List<CategoriesTree> shoperCategories, List<Tax> taxShoper, List<NewBookSQL> newBooksToAdd, int shoperMainCategory, int mainProducer, int synchronizationId)
        {
            int itemCounter = 0;

            for (int itemNumber = 0; itemNumber < newBooksToAdd.Count; itemNumber++)
            {
                NewBookSQL newBook = newBooksToAdd[itemNumber];

                // main category id
                int mainCategoryId = 0;
                int categoryCounter = -1;

                for (int i=0; i<shoperCategories.Count; i++)
                {
                    if (shoperCategories[i].Name.ToLower().Contains(newBook.MediaType.ToLower()))
                    {
                        mainCategoryId = shoperCategories[i].Id;
                        categoryCounter = i;
                    }
                }

                // subcategory id
                int subCategoryId = 0;
                for (int i=0; i < shoperCategories[categoryCounter].Children.Count; i++)
                {
                    if (shoperCategories[categoryCounter].Children[i].Name.ToLower().Contains(newBook.Category.ToLower()))
                    {
                        subCategoryId = shoperCategories[categoryCounter].Children[i].ChildId;
                    }
                }

                if (subCategoryId == 0)
                {
                    subCategoryId = mainCategoryId;
                }

                if (mainCategoryId == 0)
                {
                    mainCategoryId = subCategoryId;
                }
                
                // prices
                string priceBuy = newBook.PriceBuying.ToString();
                string priceDet = newBook.PriceDetBr.ToString();

                // avoid adding products with no price
                if (priceDet != "0.00" && priceDet != "0,00")
                {
                    // authors
                    string authors = newBook.Authors;

                    string description = $"<b>Autorzy:</b> {authors}<br><b>Typ:</b> {newBook.MediaType} <br>" +
                        $"<b>Kategoria: </b>{newBook.Category}<br><b>Rok wydania:</b> {newBook.YearOfPublish}<br>";

                    if (newBook.MediaType.ToLower() == "audiobook")
                    {
                        description += $"<b>Czas trwania: </b>{newBook.Time}<br>" +
                                $"<b>Lektorzy:</b> {newBook.Lectors}<br><br>";
                    }

                    if (newBook.MediaType.ToLower() == "ebook")
                    {
                        description += $"<b>Liczba stron: </b>{newBook.Pages}<br><br>";
                    }


                    description += $"<b>Opis: </b>{newBook.Description}";

                    var tax = taxShoper.FirstOrDefault(t => t.Value == Convert.ToInt16(newBook.TaxValue));
                    int tax_id = tax.TaxId;


                    // verify EAN code
                    string ean = newBook.Ean.Replace("-", String.Empty);
                    if (!VerifyEAN13(ean))
                    {
                        ean = "";
                    }

                    // main name - author and title. Author can be empty in Azymut.
                    string productName;
                    if (string.IsNullOrEmpty(newBook.Authors))
                    {
                        productName = newBook.Title.Length > 255 ? newBook.Title.Substring(0, 255) : newBook.Title;
                    }
                    else
                    {
                        string authorTitle = newBook.Authors + ": " + newBook.Title;
                        productName = authorTitle.Length > 255 ? authorTitle.Substring(0, 255) : authorTitle;
                    }

                    // create new shoper product to serialization
                    ShoperNewProduct newProductToAdd = new ShoperNewProduct
                    {
                        Ean = ean,
                        Isbn = newBook.Isbn,
                        Code = newBook.Code,
                        ProducerId = mainProducer,
                        TaxId = tax_id,
                        Stock = new Dictionary<string, string>
                        {
                            { "stock", "1000" },
                            { "price", priceDet },
                            { "price_buying", priceBuy }
                        },

                        TranslationsNew = new Dictionary<string, TranslationNew>
                        {
                            {
                            "pl_PL", new TranslationNew
                                {
                                    ProductName = productName,
                                    ShortDescription = newBook.ShortDescription,
                                    Description = description
                                }
                            }
                        }
                    };

                    // add categories to product
                    if (mainCategoryId != 0)
                    {
                        newProductToAdd.CategoryId = mainCategoryId;
                        newProductToAdd.Categories = new List<int> { subCategoryId };
                    }    

                    int shoperNewId = await AddProductToShoper(apiBaseUrl, token, newProductToAdd);

                    if (shoperNewId != 0)
                    {
                        if (!newBook.ImageUrl.IsNullOrEmpty())
                        {
                            int imageNewId = await AddImageToProduct(apiBaseUrl, token, shoperNewId, newBook.ImageUrl);
                        }

                        // add item do shoper integration database - code from Azymut joined with shoper id
                        NewIntegrationItem newItem = new NewIntegrationItem();
                        newItem.SynchronizationId = synchronizationId;
                        newItem.ShoperId = shoperNewId;
                        newItem.Code = newBook.Code;

                        await AddNetItemToShoperIntegration(newItem);

                        itemCounter += 1;
                    }
                    else
                    {
                        throw new Exception($" Error during adding book to Shoper - code {newBook.Code}");
                    }

                    await Task.Delay(600);
                }
            }

            return itemCounter;
        }

        public async Task<int> UpdatePricesInShoperAndSQL(string apiUrl, string token, List<PriceWithShoperId> pricesToUpdate)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            int updatedItemsCounter = 0;

            for (int itemNumber = 0; itemNumber < pricesToUpdate.Count; itemNumber++)
            {
                var newItem = new
                {
                    stock = new 
                    {
                        price = pricesToUpdate[itemNumber].PriceDetBr,
                        price_buying = pricesToUpdate[itemNumber].PriceBuying
                    }
                };

                var newItemJSON = JsonConvert.SerializeObject(newItem);
                var content = new StringContent(newItemJSON, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiUrl + $"products/{pricesToUpdate[itemNumber].ShoperId}", content);

                if (response.IsSuccessStatusCode)
                {
                    await UpdatePriceAtPricesTable(pricesToUpdate[itemNumber]);
                    updatedItemsCounter++;
                    await Task.Delay(800);
                }
            }

            return updatedItemsCounter;
        }

        public async Task<List<OrderSQL>> CreateListOfPaidOrdersAsync(string apiUrl, string token, int synchronizationId, List<BookCodeIssueType> booksCodesIssuesTypes)
        {
            // get orders with paid status
            List<OrderShoper> shoperPaidOrders = await GetShoperPaidOrdersAsync(apiUrl, token);
            List<OrderProduct> shoperOrdersProducts = await GetShoperOrdersProductsAsync(apiUrl, token);

            // create list of paid orders, with full details to send it to SQL database - to Orders table
            List<OrderSQL> ordersToSQLNotFiltered = new List<OrderSQL>();

            foreach (var shoperOrderProduct in shoperOrdersProducts)
            {
                foreach (var shoperPaidOrder in shoperPaidOrders)
                {
                    if (shoperOrderProduct.OrderId == shoperPaidOrder.OrderId)
                    {
                        foreach (var bookCodeType in booksCodesIssuesTypes)
                        {
                            if (bookCodeType.Code == shoperOrderProduct.Code)
                            {
                                OrderSQL orderSQL = new OrderSQL();
                                orderSQL.OrderId = shoperOrderProduct.OrderId;
                                orderSQL.Code = shoperOrderProduct.Code;
                                orderSQL.Status = 1;
                                orderSQL.Mail = shoperPaidOrder.Mail;
                                orderSQL.Date = shoperPaidOrder.StatusDate;
                                orderSQL.SynchronizationId = synchronizationId;
                                orderSQL.IssueId = bookCodeType.IssueId;
                                orderSQL.TypeId = bookCodeType.TypeId;
                                ordersToSQLNotFiltered.Add(orderSQL);
                            }
                        }
                    }
                }
            }
            return ordersToSQLNotFiltered;
        }

        public SmtpClient CreateSmtpClient(string mailSMTP, int mailPort, string fromAddress, string fromPassword)
        {
            var smtp = new SmtpClient
            {
                Host = mailSMTP,
                Port = mailPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress, fromPassword),
                Timeout = 20000
            };

            return smtp;
        }

        public async Task<int> SaveCategoriesToShoper(string apiUrl, string token, int mainCategory, List<string> categories)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            // Creation of SubCategories
            int order = 0;

            for (int i = 0; i < categories.Count; i++)
            {
                // read category name - main category
               
                var newItem = new CategoryNew
                {
                    ParentId = mainCategory,
                    Order = order,
                    Translations = new Dictionary<string, Translation>
                    {
                         {
                              "pl_PL", new Translation
                              {
                                  Name = categories[i],
                                  Active = 1,
                              }
                         }
                    }
                };
            
                // add new category and read id
                var newItemJSON = JsonConvert.SerializeObject(newItem);
                var content = new StringContent(newItemJSON, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl + "categories", content);

                if (!response.IsSuccessStatusCode)
                {
                    return i;
                }

                order += 1;              
                await Task.Delay(1000);
            }

            return order;
        }

        public async Task<int> UpdateCategoriesInShoperAsync(string apiUrl, string token)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(300);

            var categoryResponse = await client.GetAsync(apiUrl + $"categories-tree");
            var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
            var categoryTree = JsonConvert.DeserializeObject<List<CategoriesTree>>(categoryContent);

            List<Category> categoryListNames = await GetProductCategoriesAsync(apiUrl, token);
              
            foreach (var category2 in categoryTree)
            {
                foreach (var categoryChild in category2.Children)
                {
                    foreach (var category in categoryListNames)
                    {
                        if (categoryChild.ChildId == category.CategoryId)
                        {
                               categoryChild.Name = category.Translations["pl_PL"].Name;
                        }
                    }
                }

                foreach (var category in categoryListNames)
                {
                    if (category2.Id == category.CategoryId)
                    {
                        category2.Name = category.Translations["pl_PL"].Name;
                    }
                }
            }

            // read 40 products
            int pageCounter = 1420;
            int maxElements = 40;
            bool notLastPage = false;
            int updatedProductsCounter = 0;

            // read products
            
            do 
            {
                var response = await client.GetAsync(apiUrl + $"products?limit={maxElements}&page={pageCounter}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching products: {response.StatusCode} at page {pageCounter}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ProductsApiResponse>(content);
                List<CategoryCSV> shoperProducts = new List<CategoryCSV>();
                List<ShoperProduct> productsList = new List<ShoperProduct>();
                productsList = apiResponse.Products;

                if (productsList.Count == maxElements)
                {
                    notLastPage = true;
                    pageCounter += 1;
                }
                else
                {
                    notLastPage = false;
                }

                for (int i=0; i<productsList.Count; i++)
                {
                    ShoperProduct product = productsList[i];

                    CategoryCSV category = new CategoryCSV();
                    category.ShoperId = product.ProductId;

                    category.CodeSecond = 0;

                    if (product.Categories.Count > 1)
                    {
                        if (product.TranslationsNew["pl_PL"].Description.Contains("Typ:</b> audiobook"))
                        {

                            category.CodeFirst = 1485;
                            category.Name = "audiobook";
                            foreach (var categoryName in categoryTree[1].Children)
                            {
                                if (categoryName.ChildId == product.Categories[1])
                                {
                                    string catName = categoryName.Name;

                                    foreach (var audiobookCat in categoryTree[0].Children)
                                    {
                                        if (audiobookCat.Name == catName)
                                        {
                                            category.CodeSecond = audiobookCat.ChildId;
                                        }
                                    }
                                }
                            }
                        }

                        if (product.TranslationsNew["pl_PL"].Description.Contains("Typ:</b> ebook"))
                        {
                            category.CodeFirst = 1633;
                            category.Name = "ebook";
                            category.CodeSecond = product.Categories[1];
                        }

                        if (product.TranslationsNew["pl_PL"].Description.Contains("Typ:</b> aplikacja"))
                        {
                            category.CodeFirst = 1633;
                            category.Name = "ebook";
                            category.CodeSecond = product.Categories[1];
                        }

                        if (product.TranslationsNew["pl_PL"].Description.Contains("Typ:</b> eprasa"))
                        {
                            category.CodeFirst = 1718;
                            category.Name = "eprasa";
                            foreach (var categoryName in categoryTree[1].Children)
                            {
                                if (categoryName.ChildId == product.Categories[1])
                                {
                                    string catName = categoryName.Name;

                                    foreach (var eprasaCat in categoryTree[2].Children)
                                    {
                                        if (eprasaCat.Name == catName)
                                        {
                                            category.CodeSecond = eprasaCat.ChildId;
                                        }
                                    }

                                    if (product.Categories[1] == 1665)
                                    {
                                        category.CodeSecond = 1735;
                                    }

                                    if(product.Categories[1] == 1693)
                                    {
                                        category.CodeSecond = 1757;
                                    }
                                }
                            }
                        }

                        if (category.CodeSecond == 0)
                        {
                            throw new Exception($"Add category for {category.Name} for id = {category.ShoperId}, page counter = {pageCounter}, updated products counter = {updatedProductsCounter}, i = {i}");
                        }
                    }
                    else
                    {
                        category.CodeFirst = product.CategoryId;
                        category.CodeSecond = -1;
                    }

                    shoperProducts.Add(category);
                }

                // Api bulk list creation
                List<object> bulkRequests = new List<object>();
                int bulkCounter = 0;
                               
                foreach (var shoperProduct in shoperProducts)
                {
                    object updatedProduct = new
                    {
                        category_id = shoperProduct.CodeFirst,
                        categories = new[] { shoperProduct.CodeFirst, shoperProduct.CodeSecond }
                    };
                    

                    // creation of bulk item (one product to upgrade for bulk list)
                    var bulkItem = new
                    {
                        id = $"prod-upd-{updatedProductsCounter}",
                        path = $"/webapi/rest/products/{shoperProduct.ShoperId}",
                        method = "PUT",
                        body = updatedProduct
                    };

                    if (shoperProduct.CodeSecond != -1)
                    {
                        bulkRequests.Add(bulkItem);
                    }
                    
                    updatedProductsCounter += 1;
                    bulkCounter += 1;

                    // sending bulkRequests list with 20 elements
                    if (bulkCounter % 20 == 0 || productsList.Count < 40)
                    {
                        var serializedUpdatedProducts = JsonConvert.SerializeObject(bulkRequests);
                        var bulkContent = new StringContent(serializedUpdatedProducts, Encoding.UTF8, "application/json");
                        var bulkResponse = await client.PostAsync(apiUrl + "bulk", bulkContent);
                        var responseString = response.StatusCode.ToString();
                        if (responseString != "OK")
                        {
                            throw new Exception("Error during updating categories");
                        }
                        bulkCounter = 0;
                        bulkRequests = new List<object>();
                    }

                }

            }
            
            while (notLastPage);
            return 1;
        }
    }
}
