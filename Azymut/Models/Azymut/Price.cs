// model for creating Price item for SQL database - Prices table

namespace Azymut.Models
{
    public class Price
    {
        public int SynchronizationId { get; set; }
        public string Code { get; set; }
        public decimal PriceBuying { get; set; }
        public decimal PriceDetBr { get; set; }
        public decimal TaxValue { get; set; }
    }
}
