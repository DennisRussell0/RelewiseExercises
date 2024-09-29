// Contains the logic for downloading the product data, deserializeing it, and mapping it to the Relewise product model

// Import necessary libraries
using System.Globalization; // For parsing and formatting numbers and currencies
using System.Xml; // For handling XML data
using System.Xml.Linq; // For converting XML data to objects and vice versa (serialization and deserialization)
using Relewise.Client.DataTypes; // For the Relewise product model (e.g., Product, MultiCurrency)

// Class that handles product data mapping by implementing the IJob interface
namespace Exercise2
{
    public class ProductMapper : IJob
    {
        // The Execute method implements the job's logic (e.g., downloading, mapping)
        public async Task<string> Execute(
            JobArguments arguments, // Job-specific arguments (e.g., datasetId, apiKey)
            Func<string, Task> info, // Delegate for logging informational messages
            Func<string, Task> warn, // Delegate for logging warning messages
            CancellationToken token // Token for handling operation cancellation
        )
        {
            // URL to download the product data
            string url = "https://cdn.relewise.com/academy/productdata/googleshoppingfeed";

            // Create an instance of HttpClient to handle the HTTP request
            using HttpClient client = new();
            try
            {
                // Download the XML data asynchronously
                using var stream = await client.GetStreamAsync(url, token);

                // Log success message after downloading the data
                // await info("Product data downloaded successfully.");

                // Parse the XML data using LINQ to XML
                XDocument xmlDoc = await XDocument.LoadAsync(stream, LoadOptions.None, token);

                // Extract products from the XML feed
                var products = xmlDoc
                    .Descendants("item")
                    .Select(item => new Product
                    {
                        ProductId = item.Element(
                            XName.Get("id", "http://base.google.com/ns/1.0") // Namespace for Google Shopping feed
                        )?.Value,
                        ProductName = item.Element("title")?.Value,
                        ListPrice = item.Element(
                            XName.Get("price", "http://base.google.com/ns/1.0") // Namespace for Google Shopping feed
                        )?.Value,
                        SalesPrice = item.Element(
                            XName.Get("sale_price", "http://base.google.com/ns/1.0") // Namespace for Google Shopping feed
                        )?.Value,
                    })
                    .ToArray();

                if (products == null || products.Length == 0)
                {
                    // Handle the case where no products were found in the XML data
                    await warn("No products found in the XML data.");
                    return "Failed to deserialize products: No products found.";
                }

                // List for storing mapped Relewise products
                var relewiseProducts = new List<Relewise.Client.DataTypes.Product>();

                // Iterate over each product object in the array
                foreach (var product in products)
                {
                    // Convert prices to decimal, handling null values and removing "USD"
                    decimal salesPriceDecimal =
                        product.SalesPrice != null
                            ? decimal.Parse(
                                product.SalesPrice.Replace("USD", ""),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture
                            )
                            : 0m;

                    decimal listPriceDecimal =
                        product.ListPrice != null
                            ? decimal.Parse(
                                product.ListPrice.Replace("USD", ""),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture
                            )
                            : 0m;

                    // Create a new Relewise product and map the values from the local product object
                    var relewiseProduct = new Relewise.Client.DataTypes.Product(
                        product.ProductId ?? "Unknown"
                    )
                    {
                        // Map the product name to a multilingual field, with "en" for English
                        DisplayName = new Multilingual(
                            new Multilingual.Value("en", product.ProductName)
                        ),
                        // Map the prices (converted earlier) into MultiCurrency fields
                        ListPrice = new MultiCurrency(new Money("USD", listPriceDecimal)),
                        SalesPrice = new MultiCurrency(new Money("USD", salesPriceDecimal)),
                    };

                    // Add the mapped product to the list of Relewise products
                    relewiseProducts.Add(relewiseProduct);

                    // Log each mapped product
                    // await info($"Mapped product ID: {relewiseProduct.Id}, Title: {relewiseProduct.DisplayName}, Price: {relewiseProduct.ListPrice}, Sale Price: {relewiseProduct.SalesPrice}");
                }

                // Return success message with product count
                return $"Successfully mapped {relewiseProducts.Count} products.";
            }
            // Handle specific exceptions and log appropriate messages
            catch (HttpRequestException httpEx)
            {
                await warn($"HTTP request error: {httpEx.Message}");
                return $"Failed to download data: {httpEx.Message}";
            }
            catch (XmlException xmlEx)
            {
                await warn($"XML parsing error: {xmlEx.Message}");
                return $"Failed to parse XML data: {xmlEx.Message}";
            }
            catch (Exception ex)
            {
                await warn($"Unexpected error: {ex.Message}");
                return $"Failed to process data: {ex.Message}";
            }
        }
    }
}
