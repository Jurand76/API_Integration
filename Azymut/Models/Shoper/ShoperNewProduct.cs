using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using System.Collections.Specialized;

// model to create new Shoper item - for Shoper API POST procedure /webapi/rest/products"

namespace Azymut.Models.Shoper
{
    public class ShoperNewProduct
    {
        [JsonProperty("producer_id")]
        public int ProducerId;

        [JsonProperty("category_id")]
        public int CategoryId;

        [JsonProperty("code")]
        public string Code;

        [JsonProperty("ean")]
        public string Ean;

        [JsonProperty("additional_isbn")]
        public string Isbn;

        [JsonProperty("group_id")]
        public string? GroupId;

        [JsonProperty("unit_id")]
        public string? UnitId;

        [JsonProperty("tax_id")]
        public int? TaxId { get; set; }

        [JsonProperty("stock")]
        public Dictionary<string, string> Stock { get; set; }

        [JsonProperty("dimension_w")]
        public float? DimensionW { get; set; }

        [JsonProperty("dimension_h")]
        public float? DimensionH { get; set; }

        [JsonProperty("dimension_l")]
        public float? DimensionL { get; set; }

        [JsonProperty("categories")]
        public List<int>? Categories { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, string> Attributes { get; set; }

        [JsonProperty("translations")]
        public Dictionary<string, TranslationNew> TranslationsNew { get; set; }
    }


    public class TranslationNew
    {
        [JsonProperty("name")]
        public string ProductName;

        [JsonProperty("short_description")]
        public string ShortDescription;

        [JsonProperty("description")]
        public string Description;
    }
}
