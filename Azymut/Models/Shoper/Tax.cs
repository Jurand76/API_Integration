using Newtonsoft.Json;

// model to read taxes id from Shoper - GET procedure /webapi/rest/taxes

namespace Azymut.Models.Shoper
{
    public class TaxApiResponse
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("list")]
        public List<Tax> TaxesId { get; set; }
    }
    public class Tax
    {
        [JsonProperty("tax_id")]
        public int TaxId { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }
    }
}
