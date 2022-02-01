using System;
using System.Reflection;
using System.Threading.Tasks;
using Fhir.Proxy.Caching;
using Fhir.Proxy.Tests.Assets;
using Fhir.Proxy.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fhir.Proxy.Tests.Caching
{
    [TestClass]
    public class CacheWithRedisBackingTests
    {
        private static StorageProviderConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new StorageProviderConfig();
            root.Bind(config);

            Console.WriteLine(context.TestName);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            IHost host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddRedisCacheBackingStore(options =>
                  {
                      options.ConnectionString = config.CacheConnectionString;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(10.0);
                  });
              })
              .Build();
            host.Start();
            IJsonObjectCache cache = host.Services.GetRequiredService<IJsonObjectCache>();
            _ = cache.RemoveAsync("rediskey1").GetAwaiter().GetResult();
            _ = cache.RemoveAsync("rediskey2").GetAwaiter().GetResult();
            _ = cache.RemoveAsync("rediskey3").GetAwaiter().GetResult();
            host.StopAsync().GetAwaiter();
            host.Dispose();
        }

        [TestMethod]
        public async Task RedisCacheFromMemory_Test()
        {
            IHost host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddRedisCacheBackingStore(options =>
                  {
                      options.ConnectionString = config.CacheConnectionString;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(10.0);
                  });
              })
              .Build();
            await host.StartAsync();
            IJsonObjectCache cache = host.Services.GetRequiredService<IJsonObjectCache>();
            string value1 = "foo";
            string value2 = "bar";
            string key = "rediskey1";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync<TestJsonObject>(key, jsonObject);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            await host.StopAsync();
            host.Dispose();
        }

        [TestMethod]
        public async Task RedisCacheFromBackingStore_Test()
        {
            IHost host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddRedisCacheBackingStore(options =>
                  {
                      options.ConnectionString = config.CacheConnectionString;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(1.0);
                  });
              })
              .Build();
            await host.StartAsync();
            IJsonObjectCache cache = host.Services.GetRequiredService<IJsonObjectCache>();
            string value1 = "foo";
            string value2 = "bar";
            string key = "rediskey2";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync<TestJsonObject>(key, jsonObject);
            await Task.Delay(2000);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            await host.StopAsync();
            host.Dispose();
        }

        [TestMethod]
        public async Task RedisCacheRemove_Test()
        {
            IHost host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddRedisCacheBackingStore(options =>
                  {
                      options.ConnectionString = config.CacheConnectionString;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(10.0);
                  });
              })
              .Build();
            await host.StartAsync();
            IJsonObjectCache cache = host.Services.GetRequiredService<IJsonObjectCache>();
            string value1 = "foo";
            string value2 = "bar";
            string key = "rediskey3";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync<TestJsonObject>(key, jsonObject);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            bool removed = await cache.RemoveAsync(key);
            Assert.IsTrue(removed, "Not removed.");
            TestJsonObject joRemoved = await cache.GetAsync<TestJsonObject>(key);
            Assert.IsNull(joRemoved, "Not null.");
            await host.StopAsync();
            host.Dispose();
        }
    }
}
