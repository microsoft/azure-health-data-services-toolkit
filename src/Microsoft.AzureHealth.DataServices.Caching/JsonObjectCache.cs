using System.Collections.Concurrent;
using Microsoft.AzureHealth.DataServices.Caching.StorageProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Caching
{
    /// <summary>
    /// In-memory cache with persistent backing store for JSON objects.
    /// </summary>
    public class JsonObjectCache : IJsonObjectCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly ICacheBackingStoreProvider _provider;
        private readonly TimeSpan _expiry;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocker;

        /// <summary>
        /// Creates an instance of JsonObjectCache.
        /// </summary>
        /// <param name="options">Options for cache.</param>
        /// <param name="cache">In-memory cache.</param>
        /// <param name="provider">Cache persistence provider.</param>
        /// <param name="logger">ILogger</param>
        public JsonObjectCache(IOptions<JsonCacheOptions> options, IMemoryCache cache, ICacheBackingStoreProvider provider, ILogger<JsonObjectCache> logger = null)
        {
            _expiry = options.Value.CacheItemExpiry;
            _cache = cache;
            _provider = provider;
            _logger = logger;
            _keyLocker = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <typeparam name="T">Type of item to add.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to add to cache.</param>
        /// <returns>Task.</returns>
        public async Task AddAsync<T>(string key, T value)
        {
            SemaphoreSlim keyLock = _keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                string json = JsonConvert.SerializeObject(value);
                _cache.Set(key, json, GetOptions());
                await _provider.AddAsync(key, value);
                _logger?.LogTrace("Key {key} set to local memory cache.", key);
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
            SemaphoreSlim keyLock = _keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                string json = JsonConvert.SerializeObject(value);
                _cache.Set(key, json, GetOptions());
                await _provider.AddAsync(key, value);
                _logger?.LogTrace("Key {key} set to local memory cache.", key);
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
            SemaphoreSlim keyLock = _keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                if (_cache.TryGetValue(key, out string value))
                {
                    return JsonConvert.DeserializeObject<T>(value);
                }

                _logger?.LogTrace("Key {key} not found in local memory cache.", key);
                T remote = await _provider.GetAsync<T>(key);

                if (remote != null)
                {
                    string json = JsonConvert.SerializeObject(remote);
                    _cache.Set<string>(key, json, GetOptions());
                }

                _logger?.LogInformation("Key {key} reset from store to local memory cache.", key);
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
            SemaphoreSlim keyLock = _keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                if (_cache.TryGetValue(key, out string value))
                {
                    return value;
                }

                _logger?.LogTrace("Key {key} not found in local memory cache.", key);
                string remote = await _provider.GetAsync(key);

                if (remote != null)
                {
                    _cache.Set(key, remote, GetOptions());
                }

                _logger?.LogInformation("Key {key} reset from store to local memory cache.", key);
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
            SemaphoreSlim keyLock = _keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                _cache.Remove(key);
                return await _provider.RemoveAsync(key);
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
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_expiry.TotalMilliseconds),
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            return options;
        }

        private void OnPostEviction(object key, object letter, EvictionReason reason, object state)
        {
            _logger?.LogTrace("Key {key} evicted from cache.", key);
        }
    }
}
