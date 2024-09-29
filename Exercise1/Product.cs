// Defines the Product class which represents a product entity and is used in the ProductMapper class

using System.Text.Json.Serialization;

namespace Exercise1
{
    public class Product
    {
        // Nullable properties
        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }

        // Handle as string instead of decimal to allow for currency symbols
        [JsonPropertyName("salesPrice")]
        public string? SalesPrice { get; set; }

        [JsonPropertyName("listPrice")]
        public string? ListPrice { get; set; }
    }
}
