using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public class RedisJsonStorageProvider : IStorageProvider
    {
        public RedisJsonStorageProvider(string connectionString, string? instanceName = null)
        {
            host = Host.CreateDefaultBuilder()
              .ConfigureServices(services => services.AddStackExchangeRedisCache(options =>
              {
                  options.Configuration = connectionString;
                  options.InstanceName = instanceName ?? String.Empty;
              }))
              .Build();

            redis = host.Services.GetRequiredService<IDistributedCache>();
        }

        private readonly IHost host;
        private readonly IDistributedCache redis;

        public async Task AddAsync<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            await redis.SetStringAsync(key, json);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            string json = await redis.GetStringAsync(key);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            await redis.RemoveAsync(key);
            return true;
        }

    }
}
