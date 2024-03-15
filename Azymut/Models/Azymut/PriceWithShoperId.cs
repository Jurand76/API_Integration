// model to upgrade price in Shoper, created from SQL database, from tables Prices and ShoperIntegration

namespace Azymut.Models

{ 
    public class PriceWithShoperId
    {
        public int SynchronizationId { get; set; }
        public string Code { get; set; }
        public decimal PriceBuying { get; set; }
        public decimal PriceDetBr { get; set; }
        public decimal TaxValue { get; set; }
        public int ShoperId {  get; set; }
    }
}
