// Defines common interfaces and classes used across different jobs

namespace Exercise1
{
    // Interface for jobs
    public interface IJob
    {
        // Asynchronously handles job execution
        // Returns a Task<string> when complete
        Task<string> Execute(
            JobArguments arguments, // Job-specific arguments (e.g., datasetId, apiKey)
            Func<string, Task> info, // Delegate for logging informational messages
            Func<string, Task> warn, // Delegate for logging warning messages
            CancellationToken token // Token for handling operation cancellation
        );
    }

    // Class that holds arguments required for job execution
    public class JobArguments
    {
        // Constructor to initialize job arguments
        // Could be simplified by using the newer primary constructor syntax which allows you to define constructor parameters directly on the class itself
        public JobArguments(
            Guid datasetId, // Dataset identifier
            string apiKey, // API key for authentication
            IReadOnlyDictionary<string, string> jobConfiguration // Additional job configurations
        )
        {
            // Assigning the constructor arguments to the class properties
            DatasetId = datasetId;
            ApiKey = apiKey;
            JobConfiguration = jobConfiguration;
        }

        // Read-only properties initialized via the constructor
        public Guid DatasetId { get; } // Dataset ID
        public string ApiKey { get; } // API key for authentication
        public IReadOnlyDictionary<string, string> JobConfiguration { get; } // Configuration options
    }
}
