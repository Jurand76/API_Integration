using Newtonsoft.Json;

// model to read Categories from Shoper (Category) and create new category to write (CategoryNew)

namespace Azymut.Models.Shoper
{
    public class CategoriesApiResponse
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("list")]
        public List<Category> Categories { get; set; }
    }

    public class CategoryNew
    {
        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
                
        [JsonProperty("translations")]
        public Dictionary<string, Translation> Translations { get; set; }
    }

    public class Category
    {
        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("root")]
        public int Root { get; set; }

        [JsonProperty("translations")]
        public Dictionary<string, Translation> Translations { get; set; }
    }

    public class Translation
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("active")]
        public int Active { get; set; }
    }
}
