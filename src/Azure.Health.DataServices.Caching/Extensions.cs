using Azure.Health.DataServices.Caching.StorageProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Health.DataServices.Caching
{
    /// <summary>
    /// Data services caching extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds Azure blob storage as cache backing store.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="options">Blob storage cache options.</param>
        /// <returns>Service collection.</returns>
    public static class Extensions
    {
        public static IServiceCollection AddAzureBlobCacheBackingStore(this IServiceCollection services, Action<AzureBlobStorageCacheOptions> options)
        {
            services.AddSingleton<ICacheBackingStoreProvider, AzureJsonBlobStorageProvider>();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds Redis storage a cache backing store.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="options">Redis cache options.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddRedisCacheBackingStore(this IServiceCollection services, Action<RedisCacheOptions> options)
        {
            services.AddSingleton<ICacheBackingStoreProvider, RedisJsonStorageProvider>();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds memory cache for json objects.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="options">Json cache options.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddJsonObjectMemoryCache(this IServiceCollection services, Action<JsonCacheOptions> options)
        {
            services.AddSingleton<IJsonObjectCache, JsonObjectCache>();
            services.Configure(options);

            return services;
        }

    }
}
