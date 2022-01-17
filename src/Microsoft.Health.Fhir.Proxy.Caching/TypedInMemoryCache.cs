using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Caching.StorageProviders;

namespace Microsoft.Health.Fhir.Proxy.Caching
{
    /// <summary>
    /// An in memory cache that uses an ICacheProvider as a backing store.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedInMemoryCache<T>
    {
        /// <summary>
        /// Creates an instance of TypedInMemoryCache.
        /// </summary>
        /// <param name="cacheExpiryMilliseconds">Cache item expiry in milliseconds</param>
        /// <param name="provider">ICacheProvider</param>
        /// <param name="logger">ILogger</param>
        public TypedInMemoryCache(double cacheExpiryMilliseconds, ICacheProvider provider, ILogger<TypedInMemoryCache<T>> logger = null)
        {
            expiry = cacheExpiryMilliseconds;
            this.provider = provider;
            this.logger = logger;

            logger?.LogTrace("Cache expiry set to {expiry} milliseconds.", expiry);

            host = Host.CreateDefaultBuilder()
              .ConfigureServices(services => services.AddMemoryCache())
              .Build();

            cache = host.Services.GetRequiredService<IMemoryCache>();
        }

        private readonly ICacheProvider provider;
        private readonly IHost host;
        private readonly IMemoryCache cache;
        private readonly double expiry;
        private readonly ILogger logger;

        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Object to cache.</param>
        /// <returns>Task</returns>
        public async Task SetAsync(string key, T value)
        {
            cache.Set(key, value, GetOptions());
            await provider.AddAsync(key, value);
            logger?.LogTrace("Key {key} set to local memory cache.", key);

        }

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Object from cache.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> GetAsync(string key)
        {
            T value = cache.Get<T>(key);
            if (value == null)
            {
                logger?.LogTrace("Key {key} not found in local memory cache.", key);
                T remote = await provider.GetAsync<T>(key);
                
                if (remote != null)
                {
                    cache.Set<T>(key, remote);
                }
                value ??= remote;
                logger?.LogInformation("Key {key} reset from store to local memory cache.", key);
            }

            return value;
        }

        private MemoryCacheEntryOptions GetOptions()
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiry)
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            return options;
        }

        private async void OnPostEviction(object key, object letter, EvictionReason reason, object state)
        {
            logger?.LogTrace("Key {key} evicted from cache.", key);
            T value = await provider.GetAsync<T>((string)key);
            cache.Set<T>(key, value, GetOptions());
            logger?.LogInformation("Key {key} reset to local memory cache.", key);
        }
    }
}
