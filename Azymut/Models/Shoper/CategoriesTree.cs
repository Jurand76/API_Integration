using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Azymut.Models.Shoper

{
    public class CategoriesTree
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("children")]
        public List<CategoryChild> Children {  get; set; }

        public string Name { get; set; }
    }

    public class CategoryChild
    {
        [JsonProperty("id")]
        public int ChildId { get; set; }

        public string Name { get; set; }
    }
}
