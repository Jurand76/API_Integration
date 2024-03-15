using Newtonsoft.Json;

// model to get orders from Shoper 

namespace Azymut.Models.Shoper
{
    public class OrdersApiResponse
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("list")]
        public List<OrderShoper> Orders { get; set; }
    }
    public class OrderShoper
    {
        [JsonProperty("order_id")]
        public int OrderId;

        [JsonProperty("date")]
        public DateTime StatusDate { get; set; }

        [JsonProperty("email")]
        public string? Mail { get; set; }

        [JsonProperty("is_paid")]
        public bool IsPaid { get; set; }
    }
}
