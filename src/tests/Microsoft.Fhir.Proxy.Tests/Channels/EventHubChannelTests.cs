using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Extensions.Channels;
using Microsoft.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class EventHubChannelTests
    {
        private static EventHubConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
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

            EventHubChannel channel = new(config);
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
            EventHubChannel channel = new(config);
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
