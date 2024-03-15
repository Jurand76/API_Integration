// Model to prepare new Book to save into SQL database - table Products

namespace Azymut.Models
{    
    public class Book
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
    }
}
