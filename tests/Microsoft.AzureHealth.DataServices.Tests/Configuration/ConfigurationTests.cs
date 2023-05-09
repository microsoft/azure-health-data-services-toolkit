using System.Reflection;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    [TestClass]
    public class ConfigurationTests
    {
        private static EventHubConfig eventHubConfig;
        private static ServiceBusConfig serviceBusConfig;
        private static BlobStorageConfig blobConfig;

        public ConfigurationTests()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            eventHubConfig = new EventHubConfig();
            serviceBusConfig = new ServiceBusConfig();
            blobConfig = new BlobStorageConfig();
            root.Bind(eventHubConfig);
            root.Bind(serviceBusConfig);
            root.Bind(blobConfig);
        }

        [TestMethod]
        public void EventHubSettings_Test()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubConfig.EventHubBlobConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubConfig.EventHubBlobContainer));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubConfig.EventHubConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubConfig.EventHubName));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubConfig.EventHubProcessorContainer));
            Assert.IsTrue(eventHubConfig.EventHubSku == EventHubSkuType.Basic);
        }

        [TestMethod]
        public void ServiceBusSettings_Test()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusConfig.ServiceBusBlobConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusConfig.ServiceBusBlobContainer));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusConfig.ServiceBusConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusConfig.ServiceBusSubscription));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusConfig.ServiceBusTopic));
            Assert.IsTrue(serviceBusConfig.ServiceBusSku == ServiceBusSkuType.Standard);
        }
    }
}
