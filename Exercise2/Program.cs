// Main program to run and test the ProductMapper functionality

class Program
{
    static async Task Main(string[] args)
    {
        // Set up dummy job arguments
        var jobArguments = new Exercise2.JobArguments(
            Guid.NewGuid(),
            "my-api-key",
            new Dictionary<string, string>()
        );

        // Create an instance of ProductMapper
        var productMapper = new Exercise2.ProductMapper();

        // Execute the job and log results
        string result = await productMapper.Execute(
            jobArguments,
            message =>
            {
                Console.WriteLine("Info: " + message);
                return Task.CompletedTask;
            },
            message =>
            {
                Console.WriteLine("Warning: " + message);
                return Task.CompletedTask;
            },
            CancellationToken.None
        );

        // Output the final result of the Execute method
        Console.WriteLine(result);
    }
}
