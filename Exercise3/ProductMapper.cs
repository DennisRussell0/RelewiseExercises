// Contains the logic for downloading the product data, deserializeing it, and mapping it to the Relewise product model

// Import necessary libraries
using System.Globalization; // For parsing and formatting numbers and currencies
using Relewise.Client.DataTypes; // For the Relewise product model (e.g., Product, MultiCurrency)

namespace Exercise3
{
    public class ProductMapper : IJob
    {
        private static readonly string[] separator = ["\r\n", "\r", "\n"]; // Array of newline separators

        // The Execute method implements the job logic (downloading, parsing, and mapping)
        public async Task<string> Execute(
            JobArguments arguments, // Job-specific arguments like datasetId and apiKey
            Func<string, Task> info, // Delegate for logging informational messages
            Func<string, Task> warn, // Delegate for logging warning messages
            CancellationToken token // Token to handle operation cancellation
        )
        {
            // URL to download the product data
            string url = "https://cdn.relewise.com/academy/productdata/raw";

            using HttpClient client = new(); // Create an instance of HttpClient to send the request
            try
            {
                // Download the raw data asynchronously
                string rawData = await client.GetStringAsync(url, token);

                // Log success message after downloading the data
                // await info("Product data downloaded successfully.");

                // Split the raw data into rows based on newline characters
                var rows = rawData.Split(separator, StringSplitOptions.None);

                // Skip the first two rows (header row and separator dashes)
                var productRows = rows.Skip(2);

                // List for storing mapped Relewise products
                var relewiseProducts = new List<Relewise.Client.DataTypes.Product>();

                // Iterate over each row of product data
                foreach (var row in productRows)
                {
                    // Log the current row being processed for transparency
                    // await info($"Processing row: {row}");

                    // Skip any empty rows
                    if (string.IsNullOrWhiteSpace(row))
                    {
                        await warn("Skipping empty row.");
                        continue;
                    }

                    // Split each row into columns using the '|' character and trim whitespace
                    var columns = row.Split('|', StringSplitOptions.TrimEntries);

                    // Remove empty columns caused by leading/trailing '|'
                    columns = columns.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();

                    if (columns.Length < 6 || row.Contains("-------------"))
                    {
                        // Skip rows with insufficient columns or separator lines
                        // await warn($"Skipping invalid row: {row}");
                        continue;
                    }

                    // Map each product's properties from the relevant columns
                    string productId = columns[0].Trim(); // Product ID
                    string productName = columns[1].Trim(); // Product Name
                    string salesPriceRaw = columns[3].Trim(); // Sales Price column
                    string listPriceRaw = columns[4].Trim(); // List Price column

                    try
                    {
                        // Convert price strings to decimals, removing '$' symbols
                        decimal salesPriceDecimal = decimal.Parse(
                            salesPriceRaw.Replace("$", ""),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture
                        );
                        decimal listPriceDecimal = decimal.Parse(
                            listPriceRaw.Replace("$", ""),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture
                        );

                        // Create a new Relewise product and map values from the raw data
                        var relewiseProduct = new Relewise.Client.DataTypes.Product(productId)
                        {
                            DisplayName = new Multilingual(
                                new Multilingual.Value("en", productName)
                            ),
                            ListPrice = new MultiCurrency(new Money("USD", listPriceDecimal)),
                            SalesPrice = new MultiCurrency(new Money("USD", salesPriceDecimal)),
                        };

                        // Add the mapped product to the list
                        relewiseProducts.Add(relewiseProduct);

                        // Log each mapped product's ID and name
                        // await info($"Mapped Product ID: {productId}, Product Name: {productName}, Sales Price: {salesPriceRaw}, List Price: {listPriceRaw}");
                    }
                    catch (FormatException fex)
                    {
                        // Handle any errors while parsing prices
                        await warn($"Error parsing prices for row '{row}': {fex.Message}");
                        continue; // Skip the row if price parsing fails
                    }
                }

                // Return success message with the number of products mapped
                return $"Successfully mapped {relewiseProducts.Count} products.";
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP request errors
                await warn($"HTTP request error: {httpEx.Message}");
                return $"Failed to download data: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                await warn($"Unexpected error: {ex.Message}");
                return $"Failed to process data: {ex.Message}";
            }
        }
    }
}
