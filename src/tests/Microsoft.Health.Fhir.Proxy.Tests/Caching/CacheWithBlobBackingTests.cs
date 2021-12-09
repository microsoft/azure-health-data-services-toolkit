using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Proxy.Caching;
using Microsoft.Health.Fhir.Proxy.Caching.StorageProviders;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Health.Fhir.Proxy.Storage;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Microsoft.Health.Fhir.Proxy.Tests.Caching
{
    [TestClass]
    public class CacheWithBlobBackingTests
    {
        private static BlobStorageConfig config;
        private static string connectionString;
        private static string container = "blobbackingstore";
        private static double expiry = 1000.0;
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
            StorageBlob storage = new StorageBlob(connectionString);
            _ = storage.CreateContainerIfNotExistsAsync(container).GetAwaiter().GetResult();

            Console.WriteLine(context.TestName);
        }

        [TestMethod]
        public async Task InMemoryTest()
        {
            string value1 = "foo";
            string value2 = "bar";
            TestJsonObject jsonObject = new(value1, value2);
            string key = "blobtest1";

            IStorageProvider provider = new AzureJsonBlobStorageProvider(connectionString, container);
            TypedInMemoryCache<TestJsonObject> cache = new(expiry, provider);
            await cache.SetAsync(key, jsonObject);
            TestJsonObject actual = await cache.GetAsync(key);
            Assert.IsNotNull(actual);
            Assert.AreEqual(value1, actual.Prop1, "Prop1 mismatch.");
            Assert.AreEqual(value2, actual.Prop2, "Prop2 mismatch.");           

        }

        [TestMethod]
        public async Task InMemoryTestWithExpiry()
        {
            string value1 = "foo";
            string value2 = "bar";
            TestJsonObject jsonObject = new(value1, value2);
            string key = "blobtest2";

            IStorageProvider provider = new AzureJsonBlobStorageProvider(connectionString, container);
            TypedInMemoryCache<TestJsonObject> cache = new(expiry, provider);
            await cache.SetAsync(key, jsonObject);
            await Task.Delay(1300);
            TestJsonObject actual = await cache.GetAsync(key);
            Assert.IsNotNull(actual);
            Assert.AreEqual(value1, actual.Prop1, "Prop1 mismatch.");
            Assert.AreEqual(value2, actual.Prop2, "Prop2 mismatch.");
        }
    }
}
