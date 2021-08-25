using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Configuration;
using Microsoft.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Microsoft.Fhir.Proxy.Tests.Configuration
{
    [TestClass]
    public class ConfigurationTests
    {
        private static ServiceConfig config;
        private static EventHubConfig eventHubSettings;
        private static ServiceBusConfig serviceBusSettings;

        public ConfigurationTests()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceConfig();
            eventHubSettings = new EventHubConfig();
            serviceBusSettings = new ServiceBusConfig();
            root.Bind(config);
            root.Bind(eventHubSettings);
            root.Bind(serviceBusSettings);
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
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubSettings.EventHubBlobConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubSettings.EventHubBlobContainer));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubSettings.EventHubConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubSettings.EventHubName));
            Assert.IsTrue(!string.IsNullOrEmpty(eventHubSettings.EventHubProcessorContainer));
            Assert.IsTrue(eventHubSettings.EventHubSku == Extensions.Channels.EventHubSkuType.Basic);
        }

        [TestMethod]
        public void ServiceBusSettings_Test()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusSettings.ServiceBusBlobConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusSettings.ServiceBusBlobContainer));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusSettings.ServiceBusConnectionString));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusSettings.ServiceBusSubscription));
            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusSettings.ServiceBusTopic));
            Assert.IsTrue(serviceBusSettings.ServiceBusSku == Extensions.Channels.ServiceBusSkuType.Standard);
        }
    }
}
