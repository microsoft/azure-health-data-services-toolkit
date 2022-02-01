using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Fhir.Proxy.Channels;
using Fhir.Proxy.Extensions.Channels;
using Fhir.Proxy.Tests.Assets;
using Fhir.Proxy.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Serilog;

namespace Fhir.Proxy.Tests.Channels
{
    [TestClass]
    public class ServiceBusChannelTests
    {
        public ServiceBusChannelTests()
        {

        }

        private static ServiceBusConfig config;
        private static readonly string logPath = "../../servicebuslog.txt";
        private static Microsoft.Extensions.Logging.ILogger<ServiceBusChannel> logger;


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

            var slog = new LoggerConfiguration()
            .WriteTo.File(
            logPath,
            shared: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(10000))
            .MinimumLevel.Debug()
            .CreateLogger();

            Microsoft.Extensions.Logging.ILoggerFactory factory = LoggerFactory.Create(log =>
            {
                log.SetMinimumLevel(LogLevel.Trace);
                log.AddConsole();
                log.AddSerilog(slog);
            });

            logger = factory.CreateLogger<ServiceBusChannel>();
            factory.Dispose();

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

            IChannel outputChannel = new ServiceBusChannel(roptions, logger);

            bool completed = false;
            outputChannel.OnError += (a, args) =>
            {
                Assert.Fail("{Message} - {Stack}", args.Error.Message, args.Error.StackTrace);
            };

            outputChannel.OnReceive += (a, args) =>
            {
                string actual = Encoding.UTF8.GetString(args.Message);
                Assert.AreEqual(contentString, actual, "Content mismatch.");
                completed = true;
            };

            await inputChannel.OpenAsync();
            await outputChannel.OpenAsync();
            await outputChannel.ReceiveAsync();
            await inputChannel.SendAsync(message, new object[] { contentType });
            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            inputChannel.Dispose();
            outputChannel.Dispose();
            Assert.IsTrue(completed, "Did not complete.");
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
            IChannel sendChannel = new ServiceBusChannel(options, logger);
            Exception error = null;
            sendChannel.OnError += (a, args) =>
            {
                error = args.Error;
            };

            IChannel receiveChannel = new ServiceBusChannel(roptions, logger);
            receiveChannel.OnError += (a, args) =>
            {
                error = args.Error;
            };

            bool completed = false;
            receiveChannel.OnReceive += (a, args) =>
            {
                try
                {
                    string actual = Encoding.UTF8.GetString(args.Message);
                    LargeJsonMessage actualMsg = JsonConvert.DeserializeObject<LargeJsonMessage>(actual);

                    Assert.AreEqual(msg.Fields[0].Value, actualMsg.Fields[0].Value, "Content mismatch.");
                    completed = true;
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            };

            await receiveChannel.OpenAsync();
            await sendChannel.OpenAsync();
            await receiveChannel.ReceiveAsync();
            await sendChannel.SendAsync(message, new object[] { contentType });

            int i = 0;
            while (!completed && i < 10)
            {
                await Task.Delay(1000);
                i++;
            }

            sendChannel.Dispose();
            receiveChannel.Dispose();
            Assert.IsNull(error, "Error {0}-{1}", error?.Message, error?.StackTrace);
            Assert.IsTrue(completed, "Did not detect OnReceive event.");

        }


    }
}
