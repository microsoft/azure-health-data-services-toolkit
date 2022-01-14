using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Caching.StorageProviders;

namespace Microsoft.Health.Fhir.Proxy.Caching
{
    public class TypedInMemoryCache<T>
    {
        public TypedInMemoryCache(double cacheExpiryMilliseconds, ICacheProvider provider, ILogger logger = null)
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

        public async Task SetAsync(string key, T value)
        {
            cache.Set(key, value, GetOptions());
            await provider.AddAsync(key, value);
            logger?.LogTrace("Key {key} set to local memory cache.", key);

        }

        public async Task<T> GetAsync(string key)
        {
            T value = cache.Get<T>(key);
            if (value == null)
            {
                logger?.LogTrace("Key {key} not found in local memory cache.", key);
                T remote = await provider.GetAsync<T>(key);
                _ = remote ?? throw new Exception("Key not found in cache or persistent store.");
                cache.Set<T>(key, remote);
                value = remote;
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
