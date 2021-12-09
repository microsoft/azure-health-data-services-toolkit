using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Fhir.Proxy.Configuration;
using Microsoft.Health.Fhir.Proxy.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class AccessTokenTests
    {
        private static ServiceConfig config;
        public AccessTokenTests()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceConfig();
            root.Bind(config);
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Test()
        {
            string resource = "https://localhost";
            Authenticator auth = new(resource, config);
            //DefaultAzureCredential credential = string.IsNullOrEmpty(config.ClientId) ? new(false) : new(new DefaultAzureCredentialOptions() { ManagedIdentityClientId = config.ClientId });
            ClientSecretCredential credential = new(config.TenantId, config.ClientId, config.ClientSecret);
            string token = await auth.AquireTokenForClientAsync(credential);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Generic_Test()
        {
            string resource = "https://localhost";
            string[] scopes = new string[] { "https://localhost/.default" };
            Authenticator auth = new(resource);
            ClientSecretCredential credential = new(config.TenantId, config.ClientId, config.ClientSecret);
            //DefaultAzureCredential credential = string.IsNullOrEmpty(config.ClientId) ? new(false) : new(new DefaultAzureCredentialOptions() { ManagedIdentityClientId = config.ClientId });
            //string token = await auth.AcquireTokenForClientAsync(credential, scopes);
            string token = await auth.AquireTokenForClientAsync(credential, scopes);
            Assert.IsNotNull(token, "Security token must not be null.");
        }
    }
}
