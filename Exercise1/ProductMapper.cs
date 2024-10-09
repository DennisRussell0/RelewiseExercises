// Contains the logic for downloading the product data, deserializeing it, and mapping it to the Relewise product model

// Import necessary libraries
using System.Globalization; // For parsing and formatting numbers and currencies
using System.Text.Json; // For converting JSON data to objects and vice versa (serialization and deserialization)
using Relewise.Client.DataTypes; // For the Relewise product model (e.g., Product, MultiCurrency)

// Class that handles product data mapping by implementing the IJob interface
namespace Exercise1
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
            string url = "https://cdn.relewise.com/academy/productdata/customjsonfeed";

            // Create an instance of HttpClient to handle the HTTP request
            using HttpClient client = new();
            try
            {
                // Asynchronously download and deserialize the JSON data into an array of Product objects
                using var stream = await client.GetStreamAsync(url, token);
                Product[]? products = await JsonSerializer.DeserializeAsync<Product[]>(
                    stream,
                    cancellationToken: token
                );

                // Log success message after downloading the data
                await info("Product data downloaded successfully.");

                if (products == null || products.Length == 0)
                {
                    // Handle the case where no products were found in the JSON data
                    await warn("No products found in the JSON data.");
                    return "Failed to deserialize products: No products found.";
                }

                // List for storing mapped Relewise products
                var relewiseProducts = new List<Relewise.Client.DataTypes.Product>();

                // Iterate over each product object in the array
                foreach (var product in products)
                {
                    // Convert prices to decimal, handling null values and removing the "$" symbol
                    decimal salesPriceDecimal =
                        product.SalesPrice != null
                            ? decimal.Parse(
                                product.SalesPrice.Replace("$", ""),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture
                            )
                            : 0m;

                    decimal listPriceDecimal =
                        product.ListPrice != null
                            ? decimal.Parse(
                                product.ListPrice.Replace("$", ""),
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
                    await info(
                        $"Mapped product ID: {relewiseProduct.Id}, Name: {relewiseProduct.DisplayName}, List Price: {relewiseProduct.ListPrice}, Sale Price: {relewiseProduct.SalesPrice}"
                    );
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
            catch (JsonException jsonEx)
            {
                await warn($"JSON deserialization error: {jsonEx.Message}");
                return $"Failed to parse JSON data: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                await warn($"Unexpected error: {ex.Message}");
                return $"Failed to process data: {ex.Message}";
            }
        }
    }
}
