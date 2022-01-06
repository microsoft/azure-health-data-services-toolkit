﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.Health.Fhir.Proxy.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class ServiceBusChannelTests
    {
        public ServiceBusChannelTests()
        {

        }

        private static ServiceBusConfig config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceBusConfig();
            root.Bind(config);

            Console.WriteLine(context.TestName);
        }

        [TestCleanup]
        public async Task CleanupTest()
        {
            ServiceBusClient client = new(config.ServiceBusConnectionString);
            var receiver = client.CreateReceiver(config.ServiceBusTopic, config.ServiceBusSubscription);
            while (await receiver.PeekMessageAsync() != null)
            {
                var msg = await receiver.ReceiveMessageAsync();
                await receiver.CompleteMessageAsync(msg);
            }
        }

        [TestInitialize]
        public async Task InitialTest()
        {
            ServiceBusClient client = new(config.ServiceBusConnectionString);
            var receiver = client.CreateReceiver(config.ServiceBusTopic, config.ServiceBusSubscription);
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

            IOptions<ServiceBusSendOptions> options = Options.Create<ServiceBusSendOptions>(new ServiceBusSendOptions()
            {
                ConnectionString = config.ServiceBusConnectionString,
                Sku = config.ServiceBusSku,
                FallbackStorageConnectionString = config.ServiceBusBlobConnectionString,
                FallbackStorageContainer = config.ServiceBusBlobContainer,
                Topic = config.ServiceBusTopic,
            });

            IOptions<ServiceBusReceiveOptions> roptions = Options.Create<ServiceBusReceiveOptions>(new ServiceBusReceiveOptions()
            {
                ConnectionString = config.ServiceBusConnectionString,
                Topic = config.ServiceBusTopic,
                Subscription = config.ServiceBusSubscription,
                FallbackStorageConnectionString = config.ServiceBusBlobConnectionString,
            });

            IChannel inputChannel = new ServiceBusChannel(options);
            inputChannel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            IChannel outputChannel = new ServiceBusChannel(roptions);

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
            await Task.Delay(1000);
            await inputChannel.SendAsync(message, new object[] { contentType });
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            inputChannel.Dispose();
            Assert.IsTrue(completed);
        }

        [TestMethod]
        public async Task ServiceBusChannel_SendLargeMessage_Test()
        {
            IOptions<ServiceBusSendOptions> options = Options.Create<ServiceBusSendOptions>(new ServiceBusSendOptions()
            {
                ConnectionString = config.ServiceBusConnectionString,
                Sku = config.ServiceBusSku,
                FallbackStorageConnectionString = config.ServiceBusBlobConnectionString,
                FallbackStorageContainer = config.ServiceBusBlobContainer,
                Topic = config.ServiceBusTopic,
            });

            IOptions<ServiceBusReceiveOptions> roptions = Options.Create<ServiceBusReceiveOptions>(new ServiceBusReceiveOptions()
            {
                ConnectionString = config.ServiceBusConnectionString,
                Topic = config.ServiceBusTopic,
                Subscription = config.ServiceBusSubscription,
                FallbackStorageConnectionString = config.ServiceBusBlobConnectionString,
            });


            LargeJsonMessage msg = new();
            msg.Load(10, 300000);
            string json = JsonConvert.SerializeObject(msg);

            string contentType = "application/json";
            byte[] message = Encoding.UTF8.GetBytes(json);
            IChannel inputChannel = new ServiceBusChannel(options);
            inputChannel.OnError += (a, args) =>
            {
                Assert.Fail($"Channel error {args.Error.Message}");
            };

            IChannel outputChannel = new ServiceBusChannel(roptions);

            bool completed = false;
            outputChannel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                LargeJsonMessage actualMsg = JsonConvert.DeserializeObject<LargeJsonMessage>(actual);

                Assert.AreEqual(msg.Fields[0].Value, actualMsg.Fields[0].Value, "Content mismatch.");
                completed = true;
            };

            await outputChannel.OpenAsync();
            await outputChannel.ReceiveAsync();
            await inputChannel.OpenAsync();
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
