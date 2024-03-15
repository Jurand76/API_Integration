using Newtonsoft.Json;

namespace Azymut.Models.Shoper
{

    public class ProductsApiResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("list")]
        public List<ShoperProduct> Products { get; set; }
    }

    public class ShoperProduct
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("stock")]
        public Dictionary<string, string> Stock { get; set; }

        [JsonProperty("categories")]
        public List<int>? Categories { get; set; }

        //[JsonProperty("attributes")]
        //public Dictionary<string, string> Attributes { get; set; }

        [JsonProperty("translations")]
        public Dictionary<string, TranslationNew> TranslationsNew { get; set; }
    }
}
