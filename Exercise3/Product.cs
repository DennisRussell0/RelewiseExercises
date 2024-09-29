// Defines the Product class which represents a product entity and is used in the ProductMapper class

namespace Exercise3
{
    public class Product
    {
        // Nullable properties
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }

        // Handle as string instead of decimal to allow for currency symbols
        public string? SalesPrice { get; set; }
        public string? ListPrice { get; set; }
    }
}
