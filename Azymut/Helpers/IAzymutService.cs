using FormatSoapAPI;
using Azymut.Models;
using Azymut.Models.Azymut;


namespace Azymut.Helpers
{
    public interface IAzymutService
    {
        Task<DatabaseSync> GetLastSynchronizationAsync();
        Task SaveSynchronizationDataAsync(DatabaseSync syncParameters);
        Task SaveNewAzymutBookAsync(Book book);
        Task SaveNewAzymutPriceAsync(Price price);
        Task SaveNewOrderAsync(OrderSQL orderSQL);
        Task<List<OrderSQL>> GetOrdersFromSQLAsync(int synchronizationId, int status);
        Task<int> CheckOrderExistenceAsync(int orderId, string code, int synchronizationId);
        Task ChangeOrderStatusAsync(OrderSQL orderSQL, int status);
        Task ChangeTypeIdAsync(string code, string typeId);
        Task<List<string>> GetExistingCodesFromSQLAsync();
        Task<List<BookCodeIssueType>> GetBooksCodesIssuesAsync();
        Task<List<PriceWithShoperId>> GetExistingPricesFromSQLAsync(int synchronizationId);
        Task<string> GetAzymutSessionStringAsync(iFormatSoapAPIClient soapClient, string apiUser, string apiPassword);
        Book CreateBookForSQL(string categoryName, Kartoteka azymutProduct);
        Price CreatePriceForSQL(decimal marginPercent, Kartoteka azymutProduct);
        iFSCreateOrderReq ConvertSQLOrderToAzymut(OrderSQL orderSQL);
        PriceWithShoperId ConvertPriceToShoperModel(Kartoteka azymutProduct, decimal priceMargin, int synchronizationId);
    }
}