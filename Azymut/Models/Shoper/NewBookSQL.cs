// model to save book for Shoper - combined from SQL database tables Products and Prices

namespace Azymut.Models.Shoper
{
    public class NewBookSQL
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Ean { get; set; }
        public string Isbn { get; set; }
        public string Code { get; set; }
        public string IssueId { get; set; }
        public string Authors { get; set; }
        public string MediaType { get; set; }
        public string TypeId { get; set; }
        public string Pages { get; set; }
        public string Time { get; set; }
        public string YearOfPublish { get; set; }
        public string Lectors { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal PriceBuying { get; set; }
        public decimal PriceDetBr { get; set; }
        public decimal TaxValue { get; set; }
    }
}
