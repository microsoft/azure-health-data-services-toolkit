using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Caching;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.AzureHealth.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureHealth.DataServices.Tests.Caching
{
    [TestClass]
    public class CacheWithBlobBackingTests
    {
        private static readonly string Container = "blobbackingstore";
        private static BlobStorageConfig s_config;
        private static string s_connectionString;
        private static IHost s_host;
        private static IJsonObjectCache s_cache;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            s_config = new BlobStorageConfig();
            root.Bind(s_config);
            s_connectionString = s_config.BlobStorageChannelConnectionString;
            StorageBlob storage = new(s_connectionString);
            _ = storage.CreateContainerIfNotExistsAsync(Container).GetAwaiter().GetResult();

            s_host = Host.CreateDefaultBuilder()
              .ConfigureServices(services =>
              {
                  services.AddMemoryCache();
                  services.AddAzureBlobCacheBackingStore(options =>
                  {
                      options.ConnectionString = s_config.BlobStorageChannelConnectionString;
                      options.Container = Container;
                  });
                  services.AddJsonObjectMemoryCache(options =>
                  {
                      options.CacheItemExpiry = TimeSpan.FromSeconds(5.0);
                  });
              })
              .Build();
            s_host.Start();

            s_cache = s_host.Services.GetRequiredService<IJsonObjectCache>();

            Console.WriteLine(context.TestName);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _ = s_cache.RemoveAsync("key1").GetAwaiter().GetResult();
            _ = s_cache.RemoveAsync("key2").GetAwaiter().GetResult();
            _ = s_cache.RemoveAsync("key3").GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task BlobCacheFromMemory_Test()
        {
            string value1 = "foo1";
            string value2 = "bar2";
            string key = "key1";
            TestJsonObject jsonObject = new(value1, value2);

            await s_cache.AddAsync(key, jsonObject);
            TestJsonObject jo = await s_cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
        }

        [TestMethod]
        public async Task BlobCacheFromBackingStore_Test()
        {
            string value1 = "foo2";
            string value2 = "bar2";
            string key = "key2";
            TestJsonObject jsonObject = new(value1, value2);

            await s_cache.AddAsync(key, jsonObject);
            await Task.Delay(6000);
            TestJsonObject jo = await s_cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
        }

        [TestMethod]
        public async Task BlobCacheRemove_Test()
        {
            string value1 = "foo3";
            string value2 = "bar3";
            string key = "key3";
            TestJsonObject jsonObject = new(value1, value2);

            await s_cache.AddAsync(key, jsonObject);
            TestJsonObject jo = await s_cache.GetAsync<TestJsonObject>(key);
            Assert.AreEqual(value1, jo.Prop1, "Mismatch");
            Assert.AreEqual(value2, jo.Prop2, "Mismatch");
            bool removed = await s_cache.RemoveAsync(key);
            Assert.IsTrue(removed);
            TestJsonObject joRemoved = await s_cache.GetAsync<TestJsonObject>(key);
            Assert.IsNull(joRemoved, "Not null.");
        }
    }
}
