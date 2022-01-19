using Azure;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Storage;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    /// <summary>
    /// A cache provider for json objects that uses Azure Blob storage as a backing store.
    /// </summary>
    public class AzureJsonBlobStorageProvider : ICacheProvider
    {
        /// <summary>
        /// Creates an instance of AzureJsonBlobStorageProvider.
        /// </summary>
        /// <param name="options">Options for caching.</param>
        public AzureJsonBlobStorageProvider(IOptions<AzureBlobStorageCacheOptions> options)
        {
            storage = new(options.Value.ConnectionString);
            container = options.Value.Container;
        }

        /// <summary>
        /// Creates an instance of AzureJsonBlobStorageProvider.
        /// </summary>
        /// <param name="connectionString">Azure Blob storage connection string.</param>
        /// <param name="container">Storage container for storing cached object.</param>
        public AzureJsonBlobStorageProvider(string connectionString, string container)
        {
            storage = new(connectionString);
            this.container = container;
        }

        private readonly StorageBlob storage;
        private readonly string container;

        /// <summary>
        /// Adds an object to cache.
        /// </summary>
        /// <typeparam name="T">Type of object to cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Object to cache.</param>
        /// <returns>Task</returns>
        public async Task AddAsync<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            await storage.WriteBlockBlobAsync(container, $"{key}.json", "application/json", Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Gets an object from cache.
        /// </summary>
        /// <typeparam name="T">Type of object to get from cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Object from cache.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                byte[] content = await storage.ReadBlockBlobAsync(container, $"{key}.json");
                string contentString = Encoding.UTF8.GetString(content);
                return JsonConvert.DeserializeObject<T>(contentString);
            }
            catch (RequestFailedException)
            {
                return default;
            }
        }

        /// <summary>
        /// Removes an object from cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True if object remove otherwise false.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            return await storage.DeleteBlobAsync(container, $"{key}.json");
        }
    }
}
