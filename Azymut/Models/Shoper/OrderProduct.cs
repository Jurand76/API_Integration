using Newtonsoft.Json;

// model to get products details for orders - Shoper's method /webapi/rest/order-products

namespace Azymut.Models.Shoper
{
    public class OrdersProductsApiResponse
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("list")]
        public List<OrderProduct> OrdersProducts { get; set; }
    }

    public class OrderProduct
    {
        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
