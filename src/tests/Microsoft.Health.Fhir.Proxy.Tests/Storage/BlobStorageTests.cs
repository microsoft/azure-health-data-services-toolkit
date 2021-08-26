using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Health.Fhir.Proxy.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Storage
{
    [TestClass]
    public class BlobStorageTests
    {
        private static ConcurrentQueue<string> cleanupContainers;
        private static StorageBlob storage;
        private static readonly string alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static Random random;
        private static BlobStorageConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new BlobStorageConfig();
            root.Bind(config);
            random = new Random();
            cleanupContainers = new();
            storage = new StorageBlob(config.BlobStorageChannelConnectionString);

            Console.WriteLine(context.TestName);
        }

        [TestCleanup]
        public async Task CleanupTest()
        {
            while (!cleanupContainers.IsEmpty)
            {
                if (cleanupContainers.TryDequeue(out string container))
                {
                    await storage.DeleteContainerIfExistsAsync(container);
                }
            }
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            if (cleanupContainers.TryDequeue(out string container))
            {
                storage.DeleteContainerIfExistsAsync(container).GetAwaiter();
            }
        }

        [TestMethod]
        public async Task CreateContainerIfNotExistsTest_Test()
        {
            int expectedStatus = 201;
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            var response = await storage.CreateContainerIfNotExistsAsync(container);
            var actualStatus = response.GetRawResponse().Status;
            Assert.AreEqual(expectedStatus, actualStatus, "Status mismatch.");
        }

        [TestMethod]
        public async Task DeleteContainerIfNotExistsTest_True_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            bool result = await storage.DeleteContainerIfExistsAsync(container);
            Assert.IsTrue(result, "Container not deleted.");
        }

        [TestMethod]
        public async Task DeleteContainerIfNotExistsTest_False_Test()
        {
            string container = GetRandomName();
            bool result = await storage.DeleteContainerIfExistsAsync(container);
            Assert.IsFalse(result, "Container should not be present.");
        }

        [TestMethod]
        public async Task ListContainers_Test()
        {
            string container1 = GetRandomName();
            string container2 = GetRandomName();
            cleanupContainers.Enqueue(container1);
            cleanupContainers.Enqueue(container2);
            _ = await storage.CreateContainerIfNotExistsAsync(container1);
            _ = await storage.CreateContainerIfNotExistsAsync(container2);

            var result = storage.ListContainers();
            var en = result.GetAsyncEnumerator();
            List<string> list = new();
            while (await en.MoveNextAsync())
            {
                list.Add(en.Current.Name);
            }

            //note: some containers may not yet be deleted, but can be listed.
            Assert.IsTrue(list.Count >= 2, "Invalid number in list.");
            Assert.IsTrue(list.Contains(container1), "Container not found.");
            Assert.IsTrue(list.Contains(container2), "Container not found.");
        }

        [TestMethod]
        public async Task ContainerExists_True_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            bool result = await storage.ContainerExistsAsync(container);
            Assert.IsTrue(result, "Container not exist.");
        }

        [TestMethod]
        public async Task ContainerExists_False_Test()
        {
            string container = GetRandomName();
            bool result = await storage.ContainerExistsAsync(container);
            Assert.IsFalse(result, "Container must not exist.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_ByteArray_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            await storage.WriteBlockBlobAsync(container, blob, contentType, content);

            var result = await storage.DownloadBlockBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_ByteArray_WithMetadata_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            string propertyName = "Property1";
            string value = "Value1";
            IDictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {propertyName, value }
            };
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            await storage.WriteBlockBlobAsync(container, blob, contentType, content, null, metadata);

            var properties = await storage.GetBlobPropertiesAsync(container, blob);
            Assert.AreEqual(value, properties.Metadata[propertyName], "Mismatched metadata.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_Stream_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] contentArray = Encoding.UTF8.GetBytes(contentString);
            var stream = new MemoryStream(contentArray);
            await storage.WriteBlockBlobAsync(container, blob, contentType, stream);

            var result = await storage.DownloadBlockBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_ByteArray_WithoutAppending_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            await storage.WriteAppendBlobAsync(container, blob, contentType, content);

            var result = await storage.DownloadAppendBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_ByteArray_WithAppending_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string contentString = $"{contentString1}{contentString2}";

            await storage.WriteAppendBlobAsync(container, blob, contentType, content1);
            await storage.WriteAppendBlobAsync(container, blob, contentType, content2);

            var result = await storage.DownloadAppendBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_ByteArray_WithAppending_WithMetadata_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string propertyName = "Property1";
            string value = "Value1";
            IDictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {propertyName, value }
            };

            await storage.WriteAppendBlobAsync(container, blob, contentType, content1, null, null, metadata);
            await storage.WriteAppendBlobAsync(container, blob, contentType, content2);

            var properties = await storage.GetBlobPropertiesAsync(container, blob);
            Assert.AreEqual(value, properties.Metadata[propertyName], "Metadata mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_Stream_WithoutAppending_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            var stream = new MemoryStream(content);
            await storage.WriteAppendBlobAsync(container, blob, contentType, stream);

            var result = await storage.DownloadAppendBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_Stream_WithAppending_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string contentString = $"{contentString1}{contentString2}";
            var stream1 = new MemoryStream(content1);
            var stream2 = new MemoryStream(content2);
            await storage.WriteAppendBlobAsync(container, blob, contentType, stream1);
            await storage.WriteAppendBlobAsync(container, blob, contentType, stream2);

            var result = await storage.DownloadAppendBlobAsync(container, blob);
            string actualString = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
            Assert.AreEqual(contentType, result.Details.ContentType, "Content type mismatch.");
        }


        [TestMethod]
        public async Task ReadBlockBlob_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString = "hi";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            await storage.WriteBlockBlobAsync(container, blob, contentType, content);

            BlobOpenReadOptions options = new(false)
            {
                BufferSize = content.Length,
                Position = 0
            };

            byte[] actual = await storage.ReadBlockBlobAsync(container, blob, options);
            string actualString = Encoding.UTF8.GetString(actual);
            Assert.AreEqual(contentString, actualString, "Content mismatch");
        }

        [TestMethod]
        public async Task ReadAppendBlob_Test()
        {
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            _ = await storage.CreateContainerIfNotExistsAsync(container);
            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string contentString = $"{contentString1}{contentString2}";

            await storage.WriteAppendBlobAsync(container, blob, contentType, content1);
            await storage.WriteAppendBlobAsync(container, blob, contentType, content2);

            BlobOpenReadOptions options = new(false)
            {
                BufferSize = 100,
                Position = 0
            };

            byte[] actual = await storage.ReadAppendBlobAsync(container, blob, options);
            string actualString = Encoding.UTF8.GetString(actual);
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
        }


        private static string GetRandomName()
        {
            StringBuilder builder = new();
            int i = 0;
            while (i < 10)
            {
                builder.Append(Convert.ToString(alphabet.ToCharArray()[random.Next(0, 25)]));
                i++;
            }

            return builder.ToString();
        }



    }
}
