﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Queues.Models;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.AzureHealth.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.Channels
{
    [TestClass]
    public class EventGridChannelTests
    {
        private static EventGridConfig config;
        private static StorageBlob blobStorage;
        private static StorageQueue queueStorage;
        private static string messageQueue;
        private static string referenceQueue;
        private static DefaultAzureCredential credential;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new EventGridConfig();
            root.Bind(config);

            // Set environment variables for app registration if available
            if (!string.IsNullOrEmpty(root["ClientId"]) && !string.IsNullOrEmpty(root["TenantId"]) && !string.IsNullOrEmpty(root["ClientSecret"]))
            {
                Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", root["ClientId"]);
                Environment.SetEnvironmentVariable("AZURE_TENANT_ID", root["TenantId"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", root["ClientSecret"]);
            }

            credential = new DefaultAzureCredential();

            messageQueue = root["EventGrid_Message_Queue"] ?? Environment.GetEnvironmentVariable("PROXY_EventGrid_Message_Queue");
            referenceQueue = root["EventGrid_Reference_Queue"] ?? Environment.GetEnvironmentVariable("PROXY_EventGrid_Reference_Queue");
            blobStorage = new StorageBlob(new Uri($"https://{config.EventGridBlobStorageAccountName}.blob.core.windows.net"), credential);
            queueStorage = new StorageQueue(new Uri($"https://{config.EventGridBlobStorageAccountName}.queue.core.windows.net"), credential, null);
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
            IOptions<EventGridChannelOptions> options = Options.Create<EventGridChannelOptions>(new EventGridChannelOptions()
            {
                FallbackStorageAccountName = config.EventGridBlobStorageAccountName,
                FallbackStorageContainer = config.EventGridBlobContainer,
                TopicUriString = config.EventGridTopicUriString,
                DataVersion = config.EventGridDataVersion,
                EventType = config.EventGridEventType,
                Subject = config.EventGridSubject,
                Credential = credential,
            });

            IChannel channel = new EventGridChannel(options, null);
            channel.OnError += (i, args) =>
            {
                Assert.Fail($"Channel error {args.Error.StackTrace}");
            };
            await channel.OpenAsync();
            await queueStorage.ClearMessagesAsync(messageQueue);
            string message = "hi";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await channel.SendAsync(messageBytes);
            await Task.Delay(5000);
            QueueMessage result = await queueStorage.DequeueAsync(messageQueue, TimeSpan.FromSeconds(5.0));
            string jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result.Body.ToArray())));
            JObject jobj = JObject.Parse(jsonString);
            string b64Data = jobj["data"].Value<string>();
            string actual = Encoding.UTF8.GetString(Convert.FromBase64String(b64Data));
            Assert.AreEqual(message, actual, "Message mismatch");
        }

        [TestMethod]
        public async Task Send_LargeMessage_Test()
        {
            IOptions<EventGridChannelOptions> options = Options.Create<EventGridChannelOptions>(new EventGridChannelOptions()
            {
                FallbackStorageAccountName = config.EventGridBlobStorageAccountName,
                FallbackStorageContainer = config.EventGridBlobContainer,
                TopicUriString = config.EventGridTopicUriString,
                DataVersion = config.EventGridDataVersion,
                EventType = config.EventGridEventType,
                Subject = config.EventGridSubject,
                Credential = credential,
            });

            IChannel channel = new EventGridChannel(options, null);
            channel.OnError += (i, args) =>
            {
                Assert.Fail($"Channel error {args.Error.StackTrace}");
            };
            await channel.OpenAsync();
            Random ran = new();
            byte[] message = new byte[1500000];
            ran.NextBytes(message);
            string expected = Convert.ToBase64String(message);

            await channel.SendAsync(message);
            await Task.Delay(10000);
            QueueMessage result = await queueStorage.DequeueAsync(referenceQueue, TimeSpan.FromSeconds(5.0));
            string jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(result.Body.ToArray())));
            JObject jobj = JObject.Parse(jsonString);
            string b64Data = jobj["data"].Value<string>();
            string actualReference = Encoding.UTF8.GetString(Convert.FromBase64String(b64Data));
            string[] parts = actualReference.Split(new char[] { ',' });
            string container = parts[0];
            string blobName = parts[1];
            global::Azure.Storage.Blobs.Models.BlobDownloadResult blobResult = await blobStorage.DownloadBlockBlobAsync(container, blobName);
            string actual = Convert.ToBase64String(blobResult.Content.ToArray());
            Assert.AreEqual(expected, actual, "Message mismatch");
        }
    }
}
