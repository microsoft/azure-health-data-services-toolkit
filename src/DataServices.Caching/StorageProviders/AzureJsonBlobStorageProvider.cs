using System.Text;
using Azure;
using Azure.Identity;
using Azure.Health.DataServices.Storage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azure.Health.DataServices.Caching.StorageProviders
{
    /// <summary>
    /// A cache provider for json objects that uses Azure Blob storage as a backing store.
    /// </summary>
    public class AzureJsonBlobStorageProvider : ICacheBackingStoreProvider
    {
        /// <summary>
        /// Creates an instance of AzureJsonBlobStorageProvider.
        /// </summary>
        /// <param name="options">Options for caching.</param>
        public AzureJsonBlobStorageProvider(IOptions<AzureBlobStorageCacheOptions> options)
        {
            if (!string.IsNullOrEmpty(options.Value.ConnectionString))
            {
                storage = new(options.Value.ConnectionString);
            }
            else
            {
                storage = new(new Uri(options.Value.BlobServiceEndpoint), new DefaultAzureCredential());
            }

            container = options.Value.Container;
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
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to add.</param>
        /// <returns>Task</returns>
        public async Task AddAsync(string key, object value)
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
        /// Gets an item from the cache as a JSON string.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Item from cache as a JSON string.</returns>
        public async Task<string> GetAsync(string key)
        {
            try
            {
                byte[] content = await storage.ReadBlockBlobAsync(container, $"{key}.json");
                return Encoding.UTF8.GetString(content);
            }
            catch (RequestFailedException)
            {
                return null;
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
