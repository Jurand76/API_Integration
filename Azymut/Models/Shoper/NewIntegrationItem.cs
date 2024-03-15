// model for integration Shoper new created item (ShoperId) with existing book code in SQL database (Code)

namespace Azymut.Models.Shoper
{
    public class NewIntegrationItem
    {
        public int SynchronizationId { get; set; }
        public string Code { get; set; }
        public int ShoperId { get; set; }
    }
}
