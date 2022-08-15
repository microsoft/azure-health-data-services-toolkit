using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Storage;
using Azure.Health.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure.Health.DataServices.Tests.Channels
{
    [TestClass]
    public class BlobChannelTests
    {
        private static ConcurrentQueue<string> cleanupContainers;
        private static StorageBlob storage;
        private static readonly string alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static Random random;
        private static BlobStorageConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new BlobStorageConfig();
            root.Bind(config);
            random = new Random();
            cleanupContainers = new();
            storage = new StorageBlob(config.BlobStorageChannelConnectionString);
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

            List<string> names = await storage.ListBlobsAsync(config.BlobStorageChannelContainer);
            foreach (string blobName in names)
            {
                await storage.DeleteBlobAsync(config.BlobStorageChannelContainer, blobName);
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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { contentType, blob, container, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadBlockBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_WithoutMetadata_ConfigContainerTest()
        {
            string content = "hi";
            byte[] message = Encoding.UTF8.GetBytes(content);
            string container = GetRandomName();
            cleanupContainers.Enqueue(container);
            string blob = $"{GetRandomName()}.txt";
            string contentType = "text/plain";
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { contentType, blob, null, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadBlockBlobAsync(config.BlobStorageChannelContainer, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteBlockBlob_WithoutMetadata_EmptyParams_Test()
        {
            string content = "{ \"name\": \"hi\" }";
            byte[] message = Encoding.UTF8.GetBytes(content);
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { null, null, null, type, null, conditions, token });

            List<string> names = await storage.ListBlobsAsync(config.BlobStorageChannelContainer);

            Assert.IsTrue(names.Count == 1, "Invalid # of blobs in container.");
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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Block;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { contentType, blob, container, type, metadata, conditions, token });

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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { contentType, blob, container, type, null, conditions, token });

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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(content1, new object[] { contentType, blob, container, type, null, conditions, token });
            await channel.SendAsync(content2, new object[] { contentType, blob, container, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(container, blob);

            string actualContent = Encoding.UTF8.GetString(result.Content.ToArray());
            Assert.AreEqual(contentString, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WriteAppendBlob_WithoutMetadata_WithAppending_ConfigContainerTest()
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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(content1, new object[] { contentType, blob, null, type, null, conditions, token });
            await channel.SendAsync(content2, new object[] { contentType, blob, null, type, null, conditions, token });

            BlobDownloadResult result = await storage.DownloadAppendBlobAsync(config.BlobStorageChannelContainer, blob);

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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
            channel.OnError += (i, args) =>
            {
                Assert.Fail();
            };

            await channel.OpenAsync();
            await channel.ReceiveAsync();
            await channel.SendAsync(message, new object[] { contentType, blob, container, type, metadata, conditions, token });

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
            DataServices.Channels.BlobType type = DataServices.Channels.BlobType.Append;
            BlobRequestConditions conditions = null;
            CancellationToken token = CancellationToken.None;

            IOptions<BlobStorageChannelOptions> options = Options.Create<BlobStorageChannelOptions>(new BlobStorageChannelOptions()
            {
                ConnectionString = config.BlobStorageChannelConnectionString,
                Container = config.BlobStorageChannelContainer
            });

            IChannel channel = new BlobStorageChannel(options);
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
            await channel.SendAsync(content1, new object[] { contentType, blob, container, type, metadata, conditions, token });
            await channel.SendAsync(content2, new object[] { contentType, blob, container, type, null, conditions, token });

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
