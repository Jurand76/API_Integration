using Azymut.Models;
using Azymut.Models.Azymut;
using Azymut.Models.Shoper;
using System.Net.Mail;

namespace Azymut.Helpers
{
    public interface IShoperService
    {
        Task<string> GetTokenForApiAsync(string urlForApi, string login, string password);
        Task<List<OrderShoper>> GetShoperPaidOrdersAsync(string url, string token);
        Task<List<OrderProduct>> GetShoperOrdersProductsAsync(string url, string token);
        Task<List<NewBookSQL>> GetNewBooksToAdd(int synchronizationId);
        Task<ShoperParameters> GetShoperParametersAsync(int synchronizationId);
        Task AddNetItemToShoperIntegration(NewIntegrationItem newIntegrationItem);
        Task UpdatePriceAtPricesTable(PriceWithShoperId priceToUpdate);
        Task<List<Category>> GetProductCategoriesAsync(string url, string token);
        Task<List<CategoriesTree>> GetCategoriesTreeWithNamesShoperAsync(string apiUrl, string token);
        Task<List<Tax>> GetTaxesIdAsync(string url, string token);
        bool VerifyEAN13(string barcode);
        Task<int> UpdateOrderStatusAsync(string apiUrl, string token, int orderId, int orderStatus);
        Task<int> AddProductToShoper(string apiUrl, string token, ShoperNewProduct newProductToAdd);
        Task<int> AddImageToProduct(string apiUrl, string token, int shoperId, string imageUrl);
        Task<int> SendProductsFromSQLToShoper(string apiBaseUrl, string token, List<CategoriesTree> shoperCategories, List<Tax> taxShoper, List<NewBookSQL> newBooksToAdd, int shoperMainCategory, int mainProducer, int synchronizationId);
        Task<int> UpdatePricesInShoperAndSQL(string apiUrl, string token, List<PriceWithShoperId> pricesToUpdate);
        Task<List<OrderSQL>> CreateListOfPaidOrdersAsync(string apiUrl, string token, int synchronizationId, List<BookCodeIssueType> booksCodesIssuesTypes);
        SmtpClient CreateSmtpClient(string mailSMTP, int mailPort, string fromAddress, string fromPassword);
        Task<int> SaveCategoriesToShoper(string apiUrl, string token, int mainCategory, List<string> categories);
        Task<int> UpdateCategoriesInShoperAsync(string apiUrl, string token);
    }
}