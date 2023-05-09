using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.Clients
{
    [TestClass]
    public class FhirClientTests
    {
        private static FhirClientConfig _config;
        private static FhirClient _client;
        private static TokenCredential _credential;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            SetupTestConfiguration();
            SeedTestServer();
        }

        [TestMethod]
        public async Task GetFhirResource_Test()
        {
            var result = await _client.GetAsync("Patient", "PatientC");
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value["id"].Value<string>(), "PatientC");
        }

        [TestMethod]
        public async Task SendFhirBundle_Test()
        {
            JObject buntle = 
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value["id"].Value<string>(), "PatientC");
        }

        private static void SetupTestConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            _config = new FhirClientConfig();
            root.Bind(_config);

            _credential = new ClientSecretCredential(_config.TenantId, _config.ClientId, _config.ClientSecret);
            _client = new FhirClient(new Uri(_config.FhirServerUrl), _credential);
        }

        private static void SeedTestServer()
        {
            var bundleObject = JObject.Parse(File.ReadAllTextAsync("../../../Assets/FhirClientTestData.json").GetAwaiter().GetResult());
            _client.SendBundleAsync(bundleObject).GetAwaiter().GetResult();
        }
    }
}
