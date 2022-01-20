using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    /// <summary>
    /// A cache provider for json objects that uses redis as a backing store.
    /// </summary>
    public class RedisJsonStorageProvider : ICacheBackingStoreProvider
    {
        /// <summary>
        /// Creates an instance of RedisJsonStorageProvider.
        /// </summary>
        /// <param name="options">Redis cache options.</param>
        public RedisJsonStorageProvider(IOptions<RedisCacheOptions> options)
        {
            host = Host.CreateDefaultBuilder()
              .ConfigureServices(services => services.AddStackExchangeRedisCache(op =>
              {
                  op.Configuration = options.Value.ConnectionString;
                  op.InstanceName = options.Value.InstanceName ?? String.Empty;
              }))
              .Build();

            redis = host.Services.GetRequiredService<IDistributedCache>();
        }

        private readonly IHost host;
        private readonly IDistributedCache redis;

        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <typeparam name="T">Type of object to cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Object to cache.</param>
        /// <returns>Task</returns>
        public async Task AddAsync<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            await redis.SetStringAsync(key, json);
        }


        /// <summary>
        /// Adds a item to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to add.</param>
        /// <returns>Task</returns>
        public async Task AddAsync(string key, object value)
        {
            string json = JsonConvert.SerializeObject(value);
            await redis.SetStringAsync(key, json);
        }

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <typeparam name="T">Type of object to get from cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Object from cache.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            string json = await redis.GetStringAsync(key);
            if (json == null)
                return default;
            else
                return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Gets an item from the cache as a JSON string.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Item from cache as a JSON string.</returns>
        public async Task<string> GetAsync(string key)
        {
            return await redis.GetStringAsync(key);
        }

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True if object removed otherwise false.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            await redis.RemoveAsync(key);
            return true;
        }

    }
}
