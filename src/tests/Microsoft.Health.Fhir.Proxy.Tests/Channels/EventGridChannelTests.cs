using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Health.Fhir.Proxy.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class EventGridChannelTests
    {
        private static EventGridConfig config;
        private static StorageBlob blobStorage;
        private static StorageQueue queueStorage;
        private static string messageQueue;
        private static string referenceQueue;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new EventGridConfig();
            root.Bind(config);
            messageQueue = root["PROXY_EventGrid_Message_Queue"];
            referenceQueue = root["PROXY_EventGrid_Reference_Queue"];
            if (string.IsNullOrEmpty(messageQueue))
            {
                messageQueue = Environment.GetEnvironmentVariable("PROXY_EventGrid_Message_Queue");
                referenceQueue = Environment.GetEnvironmentVariable("PROXY_EventGrid_Reference_Queue");
            }
            blobStorage = new StorageBlob(config.EventGridBlobConnectionString);
            queueStorage = new StorageQueue(config.EventGridBlobConnectionString, null);
            Console.WriteLine(context.TestName);
            await queueStorage.CreateQueueIfNotExistsAsync(messageQueue);
            await queueStorage.CreateQueueIfNotExistsAsync(referenceQueue);

            List<string> blobs = await blobStorage.ListBlobsAsync(config.EventGridBlobContainer);
            foreach (var item in blobs)
            {
                await blobStorage.DeleteBlobAsync(config.EventGridBlobContainer, item);
            }
        }

        [ClassCleanup]
        public static async Task CleanupTestSuite()
        {
            List<string> blobs = await blobStorage.ListBlobsAsync(config.EventGridBlobContainer);
            foreach (var item in blobs)
            {
                await blobStorage.DeleteBlobAsync(config.EventGridBlobContainer, item);
            }

            await queueStorage.DeleteQueueIfExistsAsync(referenceQueue);
            await queueStorage.DeleteQueueIfExistsAsync(messageQueue);
        }

        [TestMethod]
        public async Task Send_SmallMessage_Test()
        {
            IChannel channel = new EventGridChannel(config, null);
            channel.OnError += (i, args) =>
            {
                Assert.Fail($"Channel error {args.Error.StackTrace}");
            };
            await channel.OpenAsync();
            string message = "hi";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await channel.SendAsync(messageBytes);
            await Task.Delay(5000);
            QueueMessage result = await queueStorage.DequeueAsync(messageQueue, TimeSpan.FromSeconds(10.0));
            string jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result.Body.ToArray())));
            JObject jobj = JObject.Parse(jsonString);
            string b64Data = jobj["data"].Value<string>();
            string actual = Encoding.UTF8.GetString(Convert.FromBase64String(b64Data));
            Assert.AreEqual(message, actual, "Message mismatch");
        }

        [TestMethod]
        public async Task Send_LargeMessage_Test()
        {
            IChannel channel = new EventGridChannel(config, null);
            channel.OnError += (i, args) =>
            {
                Assert.Fail($"Channel error {args.Error.StackTrace}");
            };
            await channel.OpenAsync();
            Random ran = new Random();
            byte[] message = new byte[1500000];
            ran.NextBytes(message);
            string expected = Convert.ToBase64String(message);
            await channel.SendAsync(message);
            await Task.Delay(5000);
            QueueMessage result = await queueStorage.DequeueAsync(referenceQueue, TimeSpan.FromSeconds(5.0));
            string jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result.Body.ToArray())));
            JObject jobj = JObject.Parse(jsonString);
            string b64Data = jobj["data"].Value<string>();
            string actualReference = Encoding.UTF8.GetString(Convert.FromBase64String(b64Data));
            string[] parts = actualReference.Split(new char[] { ',' });
            string container = parts[0];
            string blobName = parts[1];
            var blobResult = await blobStorage.DownloadBlockBlobAsync(container, blobName);
            string actual = Convert.ToBase64String(blobResult.Content.ToArray());
            Assert.AreEqual(expected, actual, "Message mismatch");
        }
    }
}
