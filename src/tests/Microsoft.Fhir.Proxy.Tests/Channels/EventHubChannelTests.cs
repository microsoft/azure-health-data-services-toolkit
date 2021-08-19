using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Extensions.Channels;
using Microsoft.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Fhir.Proxy.Storage;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class EventHubChannelTests
    {
        private static readonly string storageVariableName = "PROXY_STORAGE_CONNECTIONSTRING";
        private static readonly string eventHubConnectionVariableName = "PROXY_EVENTHUB_CONNECTIONSTRING";
        private static readonly string eventHubVariableName = "PROXY_EVENTHUB_NAME";
        private static readonly string eventHubSkuVariableName = "PROXY_EVENTHUB_SKU";
        private static readonly string eventHubBlobContainerName = "PROXY_EVENTHUB_BLOBCONTAINER_NAME";
        private static readonly string eventHubProcessorContainerName = "PROXY_EVENTHUB_PROCESSORCONTAINER_NAME";
        private static EventHubSettings settings;



        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(eventHubConnectionVariableName)))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Microsoft.Fhir.Proxy.Tests.Proxy.RestRequestTests).Assembly)
                    .Build();

                Environment.SetEnvironmentVariable(eventHubConnectionVariableName, configuration.GetValue<string>(eventHubConnectionVariableName));
                Environment.SetEnvironmentVariable(eventHubVariableName, configuration.GetValue<string>(eventHubVariableName));
                Environment.SetEnvironmentVariable(storageVariableName, configuration.GetValue<string>(storageVariableName));
                Environment.SetEnvironmentVariable(eventHubSkuVariableName, configuration.GetValue<string>(eventHubSkuVariableName));
                Environment.SetEnvironmentVariable(eventHubBlobContainerName, configuration.GetValue<string>(eventHubBlobContainerName));
                Environment.SetEnvironmentVariable(eventHubProcessorContainerName, configuration.GetValue<string>(eventHubProcessorContainerName));
            }

            settings = new()
            {
                BlobConnectionString = Environment.GetEnvironmentVariable(storageVariableName),
                BlobContainer = Environment.GetEnvironmentVariable(eventHubBlobContainerName),
                EventHubConnectionString = Environment.GetEnvironmentVariable(eventHubConnectionVariableName),
                EventHubName = Environment.GetEnvironmentVariable(eventHubVariableName),
                EventHubSku = (EventHubSkuType)Enum.Parse(typeof(EventHubSkuType), Environment.GetEnvironmentVariable(eventHubSkuVariableName), true),
                EventHubProcessorContainer = Environment.GetEnvironmentVariable(eventHubProcessorContainerName)
            };
        }

        [TestInitialize]
        public async Task InitialTest()
        {
            //update the checkpoint
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            var storageClient = new BlobContainerClient(Environment.GetEnvironmentVariable(storageVariableName), Environment.GetEnvironmentVariable(eventHubProcessorContainerName));
            var processor = new EventProcessorClient(storageClient, consumerGroup, Environment.GetEnvironmentVariable(eventHubConnectionVariableName), Environment.GetEnvironmentVariable(eventHubVariableName));

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
        public async Task EventHubChannel_SendSmallMessage_Test()
        {
            string contentType = "application/json";
            string propertyName = "Key1";
            string value = "Value";
            string contentString = $"{{ \"{propertyName}\": \"{value}\" }}";
            byte[] message = Encoding.UTF8.GetBytes(contentString);

            EventHubChannel channel = new(settings);
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
            await channel.ReceiveAsync();
            await Task.Delay(2000);
            await channel.SendAsync(message, new object[] { contentType });
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            channel.Dispose();
            Assert.IsTrue(completed);
        }


        [TestMethod]
        public async Task EventHubChannel_SendLargeMessage_Test()
        {
            LargeJsonMessage msg = new();
            msg.Load(10, 300000);
            string json = JsonConvert.SerializeObject(msg);

            string contentType = "application/json";
            byte[] message = Encoding.UTF8.GetBytes(json);
            EventHubChannel channel = new(settings);
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
            await channel.ReceiveAsync();
            await Task.Delay(2000);
            await channel.SendAsync(message, new object[] { contentType });

            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            channel.Dispose();
            Assert.IsTrue(completed);

        }

    }
}
