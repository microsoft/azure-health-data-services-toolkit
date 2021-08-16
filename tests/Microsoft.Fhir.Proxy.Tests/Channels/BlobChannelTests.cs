using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Extensions.Channels;
using Microsoft.Fhir.Proxy.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class BlobChannelTests
    {
        private static ConcurrentQueue<string> cleanupContainers;
        private static string connectionString;
        private static StorageBlob storage;
        private static readonly string storageVariableName = "STORAGE_CONNECTIONSTRING";
        private static readonly string alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static Random random;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            random = new Random();
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(storageVariableName)))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Microsoft.Fhir.Proxy.Tests.Proxy.RestRequestTests).Assembly)
                    .Build();

                Environment.SetEnvironmentVariable(storageVariableName, configuration.GetValue<string>(storageVariableName));
            }

            connectionString = Environment.GetEnvironmentVariable(storageVariableName);
            storage = new StorageBlob(connectionString);

            cleanupContainers = new();
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
        public async Task WriteBlockBlob_WithoutMetadata_Test()
        {
            string content = "hi";
            byte[] message = Encoding.UTF8.GetBytes(content);
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { container, blob, contentType, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadBlockBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_WithMetadata_Test()
        {
            string propertyName = "Property1";
            string value = "Value1";
            IDictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {propertyName, value }
            };

            string content = "hi";
            byte[] message = Encoding.UTF8.GetBytes(content);
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { container, blob, contentType, type, metadata, conditions, token });

            BlobDownloadResult result = await storage.DownloadBlockBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");

            var props = await storage.GetBlobPropertiesAsync(container, blob);
            Assert.AreEqual(value, props.Metadata[propertyName], "Metadata mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_WithoutMetadata_WithoutAppending_Test()
        {
            string content = "hi";
            byte[] message = Encoding.UTF8.GetBytes(content);
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { container, blob, contentType, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_WithoutMetadata_WithAppending_Test()
        {
            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string contentString = $"{contentString1}{contentString2}";

            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(content1, new object[] { container, blob, contentType, type, null, conditions, token });
            await channel.SendAsync(content2, new object[] { container, blob, contentType, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_WithMetadata_Test()
        {
            string propertyName = "Property1";
            string value = "Value1";
            IDictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {propertyName, value }
            };

            string content = "hi";
            byte[] message = Encoding.UTF8.GetBytes(content);
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { container, blob, contentType, type, metadata, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");

            var props = await storage.GetBlobPropertiesAsync(container, blob);
            Assert.AreEqual(value, props.Metadata[propertyName], "Metadata mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_WithMetadata_WithAppending_Test()
        {
            string propertyName = "Property1";
            string value = "Value1";
            IDictionary<string, string> metadata = new Dictionary<string, string>()
            {
                {propertyName, value }
            };

            string contentString1 = "hi";
            byte[] content1 = Encoding.UTF8.GetBytes(contentString1);
            string contentString2 = "\nthere";
            byte[] content2 = Encoding.UTF8.GetBytes(contentString2);
            string contentString = $"{contentString1}{contentString2}";

            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            Extensions.Channels.BlobType type = Extensions.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IChannel channel = new BlobStorageChannel(connectionString);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            channel.OnReceive += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(content1, new object[] { container, blob, contentType, type, metadata, conditions, token });
            await channel.SendAsync(content2, new object[] { container, blob, contentType, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualContent, "Content mismatch.");

            var props = await storage.GetBlobPropertiesAsync(container, blob);
            Assert.AreEqual(value, props.Metadata[propertyName], "Metadata mismatch.");
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
