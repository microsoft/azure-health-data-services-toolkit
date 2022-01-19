using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.Health.Fhir.Proxy.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class EventHubChannelTests
    {
        private static EventHubConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new EventHubConfig();
            root.Bind(config);

            Console.WriteLine(context.TestName);
        }

        [TestInitialize]
        public async Task InitialTest()
        {
            //update the checkpoint
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            var storageClient = new BlobContainerClient(config.EventHubBlobConnectionString, config.EventHubBlobContainer);
            var processor = new EventProcessorClient(storageClient, consumerGroup, config.EventHubConnectionString, config.EventHubName);

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

            IOptions<EventHubSendOptions> options = Options.Create<EventHubSendOptions>(new EventHubSendOptions()
            {
                ConnectionString = config.EventHubConnectionString,
                FallbackStorageConnectionString = config.EventHubBlobConnectionString,
                FallbackStorageContainer = config.EventHubBlobContainer,
                HubName = config.EventHubName,
                Sku = config.EventHubSku,
            });

            IOptions<EventHubReceiveOptions> roptions = Options.Create<EventHubReceiveOptions>(new EventHubReceiveOptions()
            {
                ConnectionString = config.EventHubConnectionString,
                HubName = config.EventHubName,
                StorageConnectionString = config.EventHubBlobConnectionString,
                ProcessorStorageContainer = config.EventHubProcessorContainer,
            });


            IChannel inputChannel = new EventHubChannel(options);
            inputChannel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            IChannel outputChannel = new EventHubChannel(roptions);

            bool completed = false;
            outputChannel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                Assert.AreEqual(contentString, actual, "Content mismatch.");
                completed = true;
            };

            await inputChannel.OpenAsync();
            await outputChannel.OpenAsync();
            await outputChannel.ReceiveAsync();
            await Task.Delay(2000);
            await inputChannel.SendAsync(message, new object[] { contentType });
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            inputChannel.Dispose();
            outputChannel.Dispose();
            Assert.IsTrue(completed);
        }


        [TestMethod]
        public async Task EventHubChannel_SendLargeMessage_Test()
        {
            IOptions<EventHubSendOptions> options = Options.Create<EventHubSendOptions>(new EventHubSendOptions()
            {
                ConnectionString = config.EventHubConnectionString,
                FallbackStorageConnectionString = config.EventHubBlobConnectionString,
                FallbackStorageContainer = config.EventHubBlobContainer,
                HubName = config.EventHubName,
                Sku = config.EventHubSku,
            });

            IOptions<EventHubReceiveOptions> roptions = Options.Create<EventHubReceiveOptions>(new EventHubReceiveOptions()
            {
                ConnectionString = config.EventHubConnectionString,
                HubName = config.EventHubName,
                StorageConnectionString = config.EventHubBlobConnectionString,
                ProcessorStorageContainer = config.EventHubProcessorContainer,
            });

            LargeJsonMessage msg = new();
            msg.Load(10, 300000);
            string json = JsonConvert.SerializeObject(msg);

            string contentType = "application/json";
            byte[] message = Encoding.UTF8.GetBytes(json);
            IChannel inputChannel = new EventHubChannel(options);
            inputChannel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            IChannel outputChannel = new EventHubChannel(roptions);

            bool completed = false;
            outputChannel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                LargeJsonMessage actualMsg = JsonConvert.DeserializeObject<LargeJsonMessage>(actual);

                Assert.AreEqual(msg.Fields[0].Value, actualMsg.Fields[0].Value, "Content mismatch.");
                completed = true;
            };

            await inputChannel.OpenAsync();
            await outputChannel.OpenAsync();
            await outputChannel.ReceiveAsync();
            await Task.Delay(2000);
            await inputChannel.SendAsync(message, new object[] { contentType });

            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            inputChannel.Dispose();
            outputChannel.Dispose();
            Assert.IsTrue(completed);
        }

    }
}
