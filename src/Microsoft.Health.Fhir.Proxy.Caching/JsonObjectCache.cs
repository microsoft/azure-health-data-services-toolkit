using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Caching.StorageProviders;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Caching
{
    /// <summary>
    /// In-memory cache with persistent backing store for JSON objects.
    /// </summary>
    public class JsonObjectCache : IJsonObjectCache
    {
        /// <summary>
        /// Creates an instance of JsonObjectCache.
        /// </summary>
        /// <param name="options">Options for cache.</param>
        /// <param name="cache">In-memory cache.</param>
        /// <param name="provider">Cache persistence provider.</param>
        /// <param name="logger">ILogger</param>
        public JsonObjectCache(IOptions<JsonCacheOptions> options, IMemoryCache cache, ICacheBackingStoreProvider provider, ILogger<JsonObjectCache> logger = null)
        {
            expiry = options.Value.CacheItemExpiry;
            this.cache = cache;
            this.provider = provider;
            this.logger = logger;
            keyLocker = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private readonly IMemoryCache cache;
        private readonly ILogger logger;
        private readonly ICacheBackingStoreProvider provider;
        private readonly TimeSpan expiry;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocker;

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <typeparam name="T">Type of item to add.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to add to cache.</param>
        /// <returns>Task.</returns>
        public async Task AddAsync<T>(string key, T value)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                string json = JsonConvert.SerializeObject(value);
                cache.Set(key, json, GetOptions());
                await provider.AddAsync(key, value);
                logger?.LogTrace("Key {key} set to local memory cache.", key);
            }
            finally
            {
                keyLock?.Release();
            }
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to add to cache.</param>
        /// <returns>Task.</returns>
        public async Task AddAsync(string key, object value)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                string json = JsonConvert.SerializeObject(value);
                cache.Set(key, json, GetOptions());
                await provider.AddAsync(key, value);
                logger?.LogTrace("Key {key} set to local memory cache.", key);
            }
            finally
            {
                keyLock?.Release();
            }
        }

        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <typeparam name="T">Type of item to get from cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Item from cache otherwise null.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (cache.TryGetValue(key, out string value))
                {
                    return JsonConvert.DeserializeObject<T>(value);
                }

                logger?.LogTrace("Key {key} not found in local memory cache.", key);
                T remote = await provider.GetAsync<T>(key);

                if (remote != null)
                {
                    string json = JsonConvert.SerializeObject(remote);
                    cache.Set<string>(key, json, GetOptions());
                }
                logger?.LogInformation("Key {key} reset from store to local memory cache.", key);
                return remote;
            }
            finally
            {
                keyLock?.Release();
            }
        }

        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>An item from cache otherwise null.</returns>
        public async Task<string> GetAsync(string key)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (cache.TryGetValue(key, out string value))
                {
                    return value;
                }

                logger?.LogTrace("Key {key} not found in local memory cache.", key);
                string remote = await provider.GetAsync(key);

                if (remote != null)
                {
                    cache.Set(key, remote, GetOptions());
                }

                logger?.LogInformation("Key {key} reset from store to local memory cache.", key);
                return remote;
            }
            finally
            {
                keyLock?.Release();
            }
        }

        /// <summary>
        /// Removes an item from the cache and persistence provider.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True is remove otherwise false.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                cache.Remove(key);
                return await provider.RemoveAsync(key);
            }
            finally
            {
                keyLock?.Release();
            }
        }

        private MemoryCacheEntryOptions GetOptions()
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiry.TotalMilliseconds)
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            return options;
        }

        private void OnPostEviction(object key, object letter, EvictionReason reason, object state)
        {
            logger?.LogTrace("Key {key} evicted from cache.", key);
        }
    }
}

