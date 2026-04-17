using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.AzureHealth.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Channels
{
    [TestClass]
    public class EventHubChannelTests
    {
        private static EventHubConfig config;
        private static DefaultAzureCredential credential;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new EventHubConfig();
            root.Bind(config);

            // Set environment variables for app registration if available
            if (!string.IsNullOrEmpty(root["ClientId"]) && !string.IsNullOrEmpty(root["TenantId"]) && !string.IsNullOrEmpty(root["ClientSecret"]))
            {
                Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", root["ClientId"]);
                Environment.SetEnvironmentVariable("AZURE_TENANT_ID", root["TenantId"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", root["ClientSecret"]);
            }

            credential = new DefaultAzureCredential();
            Console.WriteLine(context.TestName);
        }

        [TestInitialize]
        public async Task InitialTest()
        {
            // update the checkpoint
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            var storageClient = new BlobContainerClient(new Uri($"https://{config.EventHubBlobStorageAccountName}.blob.core.windows.net/{config.EventHubBlobContainer}"), credential);
            var processor = new EventProcessorClient(storageClient, consumerGroup, $"{config.EventHubNamespace}.servicebus.windows.net", config.EventHubName, credential);

            processor.ProcessEventAsync += async (args) =>
            {
                await args.UpdateCheckpointAsync(args.CancellationToken);
            };

            processor.ProcessErrorAsync += async (args) =>
            {
                Console.WriteLine(args.Exception.Message);
                await Task.CompletedTask;
            };

            try
            {
                await processor.StartProcessingAsync();
                await Task.Delay(4000);
                await processor.StopProcessingAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            processor = null;
        }

        [TestMethod]
        public async Task EventHubChannel_SendOnEventHubAndReceiveOnChannelSmallMessage_Test()
        {
            string contentType = "application/json";
            string propertyName = "Key1";
            string value = "Value";
            string contentString = $"{{ \"{propertyName}\": \"{value}\" }}";
            byte[] message = Encoding.UTF8.GetBytes(contentString);

            IOptions<EventHubChannelOptions> options = Options.Create<EventHubChannelOptions>(new EventHubChannelOptions()
            {
                Namespace = $"{config.EventHubNamespace}.servicebus.windows.net",

                // FallbackStorageConnectionString = config.EventHubBlobConnectionString,
                FallbackStorageContainer = config.EventHubBlobContainer,
                HubName = config.EventHubName,
                Sku = config.EventHubSku,
                ProcessorStorageContainer = config.EventHubProcessorContainer,
                Credential = credential,
                StorageAccountName = config.EventHubBlobStorageAccountName,
            });

            IChannel channel = new EventHubChannel(options);
            channel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            bool completed = false;
            channel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                Assert.AreEqual(contentString, actual, "Content mismatch.");
                completed = true;
            };

            await channel.OpenAsync();
            await Task.Delay(2000);
            await Task.Delay(2000);
            await channel.ReceiveAsync();
            await Task.Delay(5000);

            var sender = new EventHubProducerClient($"{config.EventHubNamespace}.servicebus.windows.net", config.EventHubName, credential);
            using EventDataBatch eventBatch = await sender.CreateBatchAsync();
            EventData data = new(message);
            data.ContentType = contentType;
            eventBatch.TryAdd(data);
            await sender.SendAsync(eventBatch);

            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            channel.Dispose();
            Assert.IsTrue(completed, "did not complete before timeout");
        }

        [TestMethod]
        public async Task EventHubChannel_SendSmallMessage_Test()
        {
            string contentType = "application/json";
            string propertyName = "Key1";
            string value = "Value";
            string contentString = $"{{ \"{propertyName}\": \"{value}\" }}";
            byte[] message = Encoding.UTF8.GetBytes(contentString);

            IOptions<EventHubChannelOptions> options = Options.Create<EventHubChannelOptions>(new EventHubChannelOptions()
            {
                Namespace = $"{config.EventHubNamespace}.servicebus.windows.net",
                FallbackStorageContainer = config.EventHubBlobContainer,
                HubName = config.EventHubName,
                Sku = config.EventHubSku,
                ProcessorStorageContainer = config.EventHubProcessorContainer,
                Credential = credential,
                StorageAccountName = config.EventHubBlobStorageAccountName,
            });

            IChannel channel = new EventHubChannel(options);
            channel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            bool completed = false;
            channel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                Assert.AreEqual(contentString, actual, "Content mismatch.");
                completed = true;
            };

            await channel.OpenAsync();
            await Task.Delay(2000);
            await channel.SendAsync(message, new object[] { contentType });
            await Task.Delay(2000);
            await channel.ReceiveAsync();
            await Task.Delay(5000);
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            channel.Dispose();
            Assert.IsTrue(completed, "did not complete before timeout");
        }

        [TestMethod]
        public async Task EventHubChannel_SendLargeMessage_Test()
        {
            IOptions<EventHubChannelOptions> options = Options.Create<EventHubChannelOptions>(new EventHubChannelOptions()
            {
                Namespace = $"{config.EventHubNamespace}.servicebus.windows.net",
                FallbackStorageContainer = config.EventHubBlobContainer,
                HubName = config.EventHubName,
                Sku = config.EventHubSku,
                ProcessorStorageContainer = config.EventHubProcessorContainer,
                Credential = credential,
                StorageAccountName = config.EventHubBlobStorageAccountName,
            });

            LargeJsonMessage msg = new();
            msg.Load(10, 300000);
            string json = JsonConvert.SerializeObject(msg);

            string contentType = "application/json";
            byte[] message = Encoding.UTF8.GetBytes(json);
            IChannel channel = new EventHubChannel(options);
            channel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            bool completed = false;
            channel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                LargeJsonMessage actualMsg = JsonConvert.DeserializeObject<LargeJsonMessage>(actual);

                Assert.AreEqual(msg.Fields[0].Value, actualMsg.Fields[0].Value, "Content mismatch.");
                completed = true;
            };

            await channel.OpenAsync();
            await Task.Delay(2000);
            await channel.SendAsync(message, new object[] { contentType });
            await Task.Delay(2000);
            await channel.ReceiveAsync();
            await Task.Delay(5000);
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            channel.Dispose();
            Assert.IsTrue(completed, "did not complete befor timeout.");
        }
    }
}
