using System.Reflection;
using DataServices.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataServices.Tests.Configuration
{
    [TestClass]
    public class ConfigurationTests
    {
        private static ServiceConfig config;
        private static EventHubConfig eventHubConfig;
        private static ServiceBusConfig serviceBusConfig;
        private static BlobStorageConfig blobConfig;

        public ConfigurationTests()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceConfig();
            eventHubConfig = new EventHubConfig();
            serviceBusConfig = new ServiceBusConfig();
            blobConfig = new BlobStorageConfig();
            root.Bind(config);
            root.Bind(eventHubConfig);
            root.Bind(serviceBusConfig);
            root.Bind(blobConfig);
        }

        [TestMethod]
        public void ServiceConfig_Test()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(config.ClientId), "ClientId is null.");
            Assert.IsTrue(!string.IsNullOrEmpty(config.ClientSecret), "ClientSecret is null.");
            Assert.IsTrue(!string.IsNullOrEmpty(config.FhirServerUrl), "FhirServerUrl is null.");
            Assert.IsTrue(!string.IsNullOrEmpty(config.InstrumentationKey), "InstrumentationKey is null.");
            Assert.IsTrue(!string.IsNullOrEmpty(config.KeyVaultCertificateName), "KeyVaultCertificateName is null.");
            Assert.IsTrue(!string.IsNullOrEmpty(config.KeyVaultUri), "KeyVaultUriString is null.");
            Assert.IsTrue(config.LoggingLevel == Microsoft.Extensions.Logging.LogLevel.Information);
            Assert.IsTrue(!string.IsNullOrEmpty(config.TenantId), "Tenant is null.");
            Assert.IsNotNull(config.Certficate, "Certificate is null.");
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
