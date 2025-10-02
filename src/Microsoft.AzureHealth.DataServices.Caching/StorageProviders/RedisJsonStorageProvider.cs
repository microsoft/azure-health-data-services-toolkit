using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Microsoft.AzureHealth.DataServices.Caching.StorageProviders
{
    /// <summary>
    /// A cache provider for json objects that uses redis as a backing store.
    /// </summary>
    public class RedisJsonStorageProvider : ICacheBackingStoreProvider
    {
        private readonly IHost host;
        private readonly IDistributedCache redis;
        private readonly IDatabase redisDb;

        /// <summary>
        /// Creates an instance of RedisJsonStorageProvider.
        /// </summary>
        /// <param name="options">Redis cache options.</param>
        public RedisJsonStorageProvider(IOptions<RedisCacheOptions> options)
        {
            if (!string.IsNullOrWhiteSpace(options.Value.ConnectionString))
            {
                // Case 1: Connection string
                host = Host.CreateDefaultBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddStackExchangeRedisCache(op =>
                        {
                            op.Configuration = options.Value.ConnectionString;
                            op.InstanceName = options.Value.InstanceName ?? string.Empty;
                        });
                    })
                    .Build();

                redis = host.Services.GetRequiredService<IDistributedCache>();
            }
            else if (options.Value.Credential != null)
            {
                var cacheHostName = options.Value.InstanceName;
                var configurationOptions = ConfigurationOptions.Parse($"{cacheHostName}:6380");
                configurationOptions.Ssl = true;
                configurationOptions.AbortOnConnectFail = true;
                configurationOptions = configurationOptions.ConfigureForAzureWithTokenCredentialAsync(options.Value.Credential).GetAwaiter().GetResult();
                var connectionMultiplexer = ConnectionMultiplexer.ConnectAsync(configurationOptions).GetAwaiter().GetResult();
                redisDb = connectionMultiplexer.GetDatabase();
            }
            else
            {
                // No valid Redis configuration
                throw new InvalidOperationException(
                    "No Redis connection string or DefaultAzureCredential provided for Redis cache.");
            }
        }

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
            if (redis != null)
            {
                await redis.SetStringAsync(key, json);
            }
            else if (redisDb != null)
            {
                await redisDb.StringSetAsync(key, json);
            }
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
            if (redis != null)
            {
                await redis.SetStringAsync(key, json);
            }
            else if (redisDb != null)
            {
                await redisDb.StringSetAsync(key, json);
            }
        }

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <typeparam name="T">Type of object to get from cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Object from cache.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            string json = null;

            if (redis != null)
            {
                json = await redis.GetStringAsync(key);
            }
            else if (redisDb != null)
            {
                json = await redisDb.StringGetAsync(key);
            }

            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Gets an item from the cache as a JSON string.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Item from cache as a JSON string.</returns>
        public async Task<string> GetAsync(string key)
        {
            if (redis != null)
            {
                return await redis.GetStringAsync(key);
            }
            else if (redisDb != null)
            {
                return await redisDb.StringGetAsync(key);
            }

            return null;
        }

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True if object removed otherwise false.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            if (redis != null)
            {
                await redis.RemoveAsync(key);
            }
            else if (redisDb != null)
            {
                await redisDb.KeyDeleteAsync(key);
            }

            return true;
        }
    }
}
