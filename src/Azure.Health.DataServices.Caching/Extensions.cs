using Azure.Health.DataServices.Caching.StorageProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Health.DataServices.Caching
{
    public static class Extensions
    {
        public static IServiceCollection AddAzureBlobCacheBackingStore(this IServiceCollection services, Action<AzureBlobStorageCacheOptions> options)
        {
            services.AddSingleton<ICacheBackingStoreProvider, AzureJsonBlobStorageProvider>();
            services.Configure(options);

            return services;
        }

        public static IServiceCollection AddRedisCacheBackingStore(this IServiceCollection services, Action<RedisCacheOptions> options)
        {
            services.AddSingleton<ICacheBackingStoreProvider, RedisJsonStorageProvider>();
            services.Configure(options);

            return services;
        }


        public static IServiceCollection AddJsonObjectMemoryCache(this IServiceCollection services, Action<JsonCacheOptions> options)
        {
            services.AddSingleton<IJsonObjectCache, JsonObjectCache>();
            services.Configure(options);

            return services;
        }

    }
}
