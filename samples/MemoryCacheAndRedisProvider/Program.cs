using DataServices.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MemoryCacheAndRedisProvider
{
    public class Program
    {
        private static MyServiceConfig config;
        static async Task Main()
        {
            IConfigurationBuilder cbuilder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = cbuilder.Build();
            config = new();
            root.Bind(config);

            IHostBuilder builder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMemoryCache();
                    services.AddRedisCacheBackingStore(options =>
                    {
                        options.ConnectionString = config.ConnectionString;
                    });
                    services.AddJsonObjectMemoryCache(options =>
                    {
                        options.CacheItemExpiry = TimeSpan.FromSeconds(5.0);
                    });
                    services.AddScoped<IMyService, MyService>();
                });

            var app = builder.Build();
            app.RunAsync().GetAwaiter();

            string cacheKey = "key1";
            TestCacheItem item = new() { Name = "testitem", Value = "testvalue" };
            IMyService myservice = app.Services.GetRequiredService<IMyService>();
            await myservice.SetAsync<TestCacheItem>(cacheKey, item);

            var cachedItem = await myservice.GetAsync<TestCacheItem>(cacheKey);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Cached item....\r\nName {cachedItem.Name}\r\nValue {cachedItem.Value}");
            Console.ResetColor();
        }
    }
}
