using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Extensions.Channels;
using Microsoft.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class ServiceBusChannelTests
    {
        public ServiceBusChannelTests()
        {

        }

        private static readonly string storageName = "PROXY_STORAGE_CONNECTIONSTRING";
        private static readonly string servieBusConnectionName = "PROXY_SERVICEBUS_CONNECTIONSTRING";
        private static readonly string topicName = "PROXY_SERVICEBUS_TOPIC";
        private static readonly string subscriptionName = "PROXY_SERVICEBUS_SUBSCRIPTION";
        private static readonly string serviceBusBlobContainerName = "PROXY_SERVICEBUS_BLOBCONTAINER_NAME";
        private static readonly string serviceBusSku = "PROXY_SERVICEBUS_SKU";
        private static ServiceBusSettings settings;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(servieBusConnectionName)))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Microsoft.Fhir.Proxy.Tests.Proxy.RestRequestTests).Assembly)
                    .Build();

                Environment.SetEnvironmentVariable(servieBusConnectionName, configuration.GetValue<string>(servieBusConnectionName));
                Environment.SetEnvironmentVariable(storageName, configuration.GetValue<string>(storageName));
                Environment.SetEnvironmentVariable(serviceBusSku, configuration.GetValue<string>(serviceBusSku));
                Environment.SetEnvironmentVariable(serviceBusBlobContainerName, configuration.GetValue<string>(serviceBusBlobContainerName));
                Environment.SetEnvironmentVariable(topicName, configuration.GetValue<string>(topicName));
                Environment.SetEnvironmentVariable(subscriptionName, configuration.GetValue<string>(subscriptionName));
            }

            settings = new()
            {
                ServiceBusBlobConnectionString = Environment.GetEnvironmentVariable(storageName),
                ServiceBusBlobContainer = Environment.GetEnvironmentVariable(serviceBusBlobContainerName),
                ServiceBusConnectionString = Environment.GetEnvironmentVariable(servieBusConnectionName),
                ServiceBusSku = (ServiceBusSkuType)Enum.Parse(typeof(ServiceBusSkuType), Environment.GetEnvironmentVariable(serviceBusSku), true),
                ServiceBusTopic = Environment.GetEnvironmentVariable(topicName),
                ServiceBusSubscription = Environment.GetEnvironmentVariable(subscriptionName),
            };
        }

        [TestCleanup]
        public async Task CleanupTest()
        {
            ServiceBusClient client = new(settings.ServiceBusConnectionString);
            var receiver = client.CreateReceiver(settings.ServiceBusTopic, settings.ServiceBusSubscription);
            while (await receiver.PeekMessageAsync() != null)
            {
                var msg = await receiver.ReceiveMessageAsync();
                await receiver.CompleteMessageAsync(msg);
            }
        }

        [TestInitialize]
        public async Task InitialTest()
        {
            ServiceBusClient client = new(settings.ServiceBusConnectionString);
            var receiver = client.CreateReceiver(settings.ServiceBusTopic, settings.ServiceBusSubscription);
            while (await receiver.PeekMessageAsync() != null)
            {
                var msg = await receiver.ReceiveMessageAsync();
                await receiver.CompleteMessageAsync(msg);
            }
        }

        [TestMethod]
        public async Task ServiceBusChannel_SendSmallMessage_Test()
        {
            string contentType = "application/json";
            string propertyName = "Key1";
            string value = "Value";
            string contentString = $"{{ \"{propertyName}\": \"{value}\" }}";
            byte[] message = Encoding.UTF8.GetBytes(contentString);

            ServiceBusChannel channel = new(settings);
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
            await Task.Delay(1000);
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
        public async Task ServiceBusChannel_SendLargeMessage_Test()
        {
            LargeJsonMessage msg = new();
            msg.Load(10, 300000);
            string json = JsonConvert.SerializeObject(msg);

            string contentType = "application/json";
            byte[] message = Encoding.UTF8.GetBytes(json);
            ServiceBusChannel channel = new(settings);
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
            await Task.Delay(1000);
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
