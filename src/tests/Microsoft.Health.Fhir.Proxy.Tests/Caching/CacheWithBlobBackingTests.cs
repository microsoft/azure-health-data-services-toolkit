using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Caching;
using Microsoft.Health.Fhir.Proxy.Caching.StorageProviders;
using Microsoft.Health.Fhir.Proxy.Storage;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.Health.Fhir.Proxy.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Microsoft.Health.Fhir.Proxy.Tests.Caching
{
    [TestClass]
    public class CacheWithBlobBackingTests
    {
        private static BlobStorageConfig config;
        private static string connectionString;
        private static readonly string container = "blobbackingstore";
        private static IHost host;
        private static IJsonObjectCache cache;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new BlobStorageConfig();
            root.Bind(config);
            connectionString = config.BlobStorageChannelConnectionString;
            StorageBlob storage = new(connectionString);
            _ = storage.CreateContainerIfNotExistsAsync(container).GetAwaiter().GetResult();

            host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddAzureBlobCacheBackingStore(options =>
                  {
                      options.ConnectionString = config.BlobStorageChannelConnectionString;
                      options.Container = container;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(5.0);
                  });
              })
              .Build();
            host.Start();

            cache = host.Services.GetRequiredService<IJsonObjectCache>();

            Console.WriteLine(context.TestName);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            //IHost host = Host.CreateDefaultBuilder()
            //  .ConfigureServices(services =>
            //  {
            //      services.AddMemoryCache();
            //      services.AddAzureBlobCacheBackingStore(options =>
            //      {
            //          options.ConnectionString = config.BlobStorageChannelConnectionString;
            //          options.Container = container;
            //      });
            //      services.AddJsonObjectMemoryCache(options =>
            //      {
            //          options.CacheItemExpiry = TimeSpan.FromSeconds(1.0);
            //      });
            //  })
            //  .Build();
            //host.Start();
            //IJsonObjectMemoryCache cache = host.Services.GetRequiredService<IJsonObjectMemoryCache>();
            _ = cache.RemoveAsync("key1").GetAwaiter().GetResult();
            _ = cache.RemoveAsync("key2").GetAwaiter().GetResult();
            _ = cache.RemoveAsync("key3").GetAwaiter().GetResult();
            //host.StopAsync().GetAwaiter();
            //host.Dispose();
        }

        [TestMethod]
        public async Task BlobCacheFromMemory_Test()
        {
            //IHost host = Host.CreateDefaultBuilder()
            //  .ConfigureServices(services =>
            //  {
            //      services.AddMemoryCache();
            //      services.AddAzureBlobCacheBackingStore(options =>
            //      {
            //          options.ConnectionString = config.BlobStorageChannelConnectionString;
            //          options.Container = container;
            //      });
            //      services.AddJsonObjectMemoryCache(options =>
            //      {
            //          options.CacheItemExpiry = TimeSpan.FromSeconds(1.0);
            //      });
            //  })
            //  .Build();
            //await host.StartAsync();
            //IJsonObjectMemoryCache cache = host.Services.GetRequiredService<IJsonObjectMemoryCache>();
            string value1 = "foo1";
            string value2 = "bar2";
            string key = "key1";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync(key, jsonObject);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            //await host.StopAsync();
            //host.Dispose();
        }

        [TestMethod]
        public async Task BlobCacheFromBackingStore_Test()
        {
            //IHost host = Host.CreateDefaultBuilder()
            //  .ConfigureServices(services =>
            //  {
            //      services.AddMemoryCache();
            //      services.AddAzureBlobCacheBackingStore(options =>
            //      {
            //          options.ConnectionString = config.BlobStorageChannelConnectionString;
            //          options.Container = container;
            //      });
            //      services.AddJsonObjectMemoryCache(options =>
            //      {
            //          options.CacheItemExpiry = TimeSpan.FromSeconds(1.0);
            //      });
            //  })
            //  .Build();
            //await host.StartAsync();
            //IJsonObjectMemoryCache cache = host.Services.GetRequiredService<IJsonObjectMemoryCache>();
            string value1 = "foo2";
            string value2 = "bar2";
            string key = "key2";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync(key, jsonObject);
            await Task.Delay(6000);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            //await host.StopAsync();
            //host.Dispose();
        }

        [TestMethod]
        public async Task BlobCacheRemove_Test()
        {
            //IHost host = Host.CreateDefaultBuilder()
            //  .ConfigureServices(services =>
            //  {
            //      services.AddMemoryCache();
            //      services.AddAzureBlobCacheBackingStore(options =>
            //      {
            //          options.ConnectionString = config.BlobStorageChannelConnectionString;
            //          options.Container = config.BlobStorageChannelContainer;
            //      });
            //      services.AddJsonObjectMemoryCache(options =>
            //      {
            //          options.CacheItemExpiry = TimeSpan.FromSeconds(5.0);
            //      });
            //  })
            //  .Build();
            //await host.StartAsync();
            //IJsonObjectMemoryCache cache = host.Services.GetRequiredService<IJsonObjectMemoryCache>();
            string value1 = "foo3";
            string value2 = "bar3";
            string key = "key3";
            TestJsonObject jsonObject = new(value1, value2);

            await cache.AddAsync(key, jsonObject);
            TestJsonObject jo = await cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            bool removed = await cache.RemoveAsync(key);
            Assert.IsTrue(removed);
            TestJsonObject joRemoved = await cache.GetAsync<TestJsonObject>(key);
            Assert.IsNull(joRemoved, "Not null.");
            //await host.StopAsync();
            //host.Dispose();
        }
    }
}
