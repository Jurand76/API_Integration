using FormatSoapAPI;
using Azymut.Models;
using DataAccessLibrary.Databases;
using Azymut.Models.Azymut;


namespace Azymut.Helpers
{
    public class AzymutService : IAzymutService
    {

        private readonly ISqlData _db;
        private readonly string _azymutConnectionString;

        public AzymutService(ISqlData db, IConfiguration configuration)
        {
            _db = db;
            _azymutConnectionString = configuration.GetConnectionString("AzymutConnection");
        }

        public async Task<DatabaseSync> GetLastSynchronizationAsync()
        {
            DatabaseSync lastSynchronization = await _db.GetDataTypeAsync<DatabaseSync, dynamic>(_azymutConnectionString, "dbo.sp_GetLastSyncDate", null);
            return lastSynchronization;
        }

        public async Task SaveSynchronizationDataAsync(DatabaseSync syncParameters)
        {
            await _db.SaveDataAsync<DatabaseSync>(_azymutConnectionString, "dbo.sp_SaveSyncDate", syncParameters);
        }

        public async Task SaveNewAzymutBookAsync(Book book)
        {
            await _db.SaveDataAsync<Book>(_azymutConnectionString, "dbo.sp_SaveNewBook", book);
        }

        public async Task SaveNewAzymutPriceAsync(Price price)
        {
            await _db.SaveDataAsync<Price>(_azymutConnectionString, "dbo.sp_SaveNewPrice", price);
        }

        public async Task SaveNewOrderAsync(OrderSQL orderSQL)
        {
            await _db.SaveDataAsync<OrderSQL>(_azymutConnectionString, "dbo.sp_SaveNewOrder", orderSQL);
        }

        public async Task<List<OrderSQL>> GetOrdersFromSQLAsync(int synchronizationId, int status)
        {
            List<OrderSQL> newOrders = await _db.GetDataListAsync<OrderSQL, dynamic>(_azymutConnectionString, "dbo.sp_GetOrdersWithStatus", new { @synchronizationId = synchronizationId, @status = status });
            return newOrders;
        }

        public async Task<int> CheckOrderExistenceAsync(int orderId, string code, int synchronizationId)
        {
            int response = await _db.GetDataTypeAsync<int, dynamic>(_azymutConnectionString, "dbo.sp_CheckOrderExistence", new { @OrderId = orderId, @SynchronizationId = synchronizationId, @Code = code });
            return response;
        }

        public async Task ChangeOrderStatusAsync(OrderSQL orderSQL, int status)
        {
            await _db.SaveDataAsync<dynamic>(_azymutConnectionString, "dbo.sp_ChangeOrderStatus",
                new { @orderId = orderSQL.OrderId, @synchronizationId = orderSQL.SynchronizationId, @code = orderSQL.Code, @newStatus = status });
        }

        public async Task ChangeTypeIdAsync(string code, string typeId)
        {
            await _db.SaveDataAsync<dynamic>(_azymutConnectionString, "dbo.sp_UpdateAzymutTypeId",
                new { @Code = code, @TypeId = typeId });
        }

        public async Task<string> GetAzymutSessionStringAsync(iFormatSoapAPIClient soapClient, string apiUser, string apiPassword)
        {

            getSession session = new getSession();
            session.username = apiUser;
            session.password = apiPassword;
            var response = await soapClient.getSessionAsync(session);
            string sessionId = response.getSessionResponse.getSessionResult.result.sessionID;

            return sessionId;
        }

        public async Task<List<string>> GetExistingCodesFromSQLAsync()
        {
            List<string> existingCodes = await _db.GetDataListAsync<string, dynamic>(_azymutConnectionString, "dbo.sp_GetExistingBooksCodes", null);
            return existingCodes;
        }

        public async Task<List<PriceWithShoperId>> GetExistingPricesFromSQLAsync(int synchronizationId)
        {
            List<PriceWithShoperId> existingPrices = await _db.GetDataListAsync<PriceWithShoperId, dynamic>(_azymutConnectionString, "dbo.sp_GetExistingPrices", new { @synchronizationId = synchronizationId });
            return existingPrices;
        }

        public async Task<List<BookCodeIssueType>> GetBooksCodesIssuesAsync()
        {
            List<BookCodeIssueType> booksCodesIssuesTypes = await _db.GetDataListAsync<BookCodeIssueType, dynamic>(_azymutConnectionString, "dbo.sp_GetExistingBooksCodesIssuesAndTypes", null);
            return booksCodesIssuesTypes;
        }

        public Book CreateBookForSQL(string categoryName, Kartoteka azymutProduct)
        {
            // authors
            string authors = "";
            for (int i = 0; i < azymutProduct.Tworcy.Autorzy.NazwaAutora.Count; i++)
            {
                authors += azymutProduct.Tworcy.Autorzy.NazwaAutora[i];
                if (i + 1 < azymutProduct.Tworcy.Autorzy.NazwaAutora.Count)
                {
                    authors = authors + ", ";
                }
            }

            // lectors
            string lectors = "";
            for (int i = 0; i < azymutProduct.Tworcy.Lektorzy.NazwaLektora.Count; i++)
            {
                lectors += azymutProduct.Tworcy.Lektorzy.NazwaLektora[i];
                if (i + 1 < azymutProduct.Tworcy.Lektorzy.NazwaLektora.Count)
                {
                    lectors = lectors + ", ";
                }
            }

            // create new book with all parameters to add
            Book newBookToAdd = new Book();
            newBookToAdd.Title = azymutProduct.Tytul;
            newBookToAdd.Category = categoryName;
            newBookToAdd.Ean = azymutProduct.Ean;
            newBookToAdd.Isbn = azymutProduct.Isbn;
            newBookToAdd.Code = azymutProduct.Id;
            newBookToAdd.MediaType = azymutProduct.Typ.Nazwa.ToLower();
            if (azymutProduct.Issues.IssueList.Count != 0)
            {
                newBookToAdd.IssueId = azymutProduct.Issues.IssueList[0].IssueId;
                newBookToAdd.TypeId = azymutProduct.Issues.IssueList[0].IssueAtrybuty.Format;
            }
            else
            {
                newBookToAdd.IssueId = "";
                if (newBookToAdd.MediaType == "ebook" || newBookToAdd.MediaType == "eprasa")
                {
                    newBookToAdd.TypeId = "epub";
                }
                else
                {
                    newBookToAdd.TypeId = "mp3";
                }
            }
            newBookToAdd.Authors = authors;
            newBookToAdd.Pages = azymutProduct.Atrybuty.Strony;
            newBookToAdd.Time = azymutProduct.Atrybuty.Czas;
            newBookToAdd.YearOfPublish = azymutProduct.RokWydania;
            newBookToAdd.Lectors = lectors;
            newBookToAdd.ShortDescription = azymutProduct.OpisKrotki;
            newBookToAdd.Description = azymutProduct.Opis;
            if (azymutProduct.Skany.Urls.Count != 0)
            {
                newBookToAdd.ImageUrl = azymutProduct.Skany.Urls[0].Link;
            }
            else
            {
                newBookToAdd.ImageUrl = "";
            }

            return newBookToAdd;
        }

        public Price CreatePriceForSQL(decimal marginPercent, Kartoteka azymutProduct)
        {
            Price newPriceToAdd = new Price();

            newPriceToAdd.Code = azymutProduct.Id;
            if (azymutProduct.Issues.IssueList.Count != 0)
            {
                newPriceToAdd.PriceBuying = azymutProduct.Issues.IssueList[0].CenaZakBr;
                newPriceToAdd.PriceDetBr = newPriceToAdd.PriceBuying * ((100 + marginPercent) / 100);
                newPriceToAdd.TaxValue = azymutProduct.Issues.IssueList[0].VatProc.Procent;
            }
            else
            {
                newPriceToAdd.PriceBuying = 0;
                newPriceToAdd.PriceDetBr = 0;
                newPriceToAdd.TaxValue = 0;
            }

            return newPriceToAdd;
        }

        public iFSCreateOrderReq ConvertSQLOrderToAzymut(OrderSQL orderSQL)
        {
            iFSCreateOrderReq orderReq = new iFSCreateOrderReq();
            orderReq.issueID = orderSQL.IssueId;
            orderReq.productID = orderSQL.Code;
            orderReq.typeID = orderSQL.TypeId;

            return orderReq;
        }

        public PriceWithShoperId ConvertPriceToShoperModel(Kartoteka azymutProduct, decimal priceMargin, int synchronizationId)
        {
            PriceWithShoperId azymutPrice = new PriceWithShoperId();
            azymutPrice.PriceBuying = azymutProduct.Issues.IssueList[0].CenaZakBr;
            azymutPrice.TaxValue = azymutProduct.Issues.IssueList[0].VatProc.Procent;
            azymutPrice.Code = azymutProduct.Id;
            azymutPrice.PriceDetBr = azymutPrice.PriceBuying * (100 + priceMargin) / 100;
            azymutPrice.SynchronizationId = synchronizationId;

            return azymutPrice;             
        }
        
    }
}
