using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Configuration;
using Microsoft.Fhir.Proxy.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class AccessTokenTests
    {
        private static ServiceConfig config;
        public AccessTokenTests()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceConfig();
            root.Bind(config);
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Test()
        {
            string resource = "https://localhost";
            Authenticator auth = new Authenticator(resource, config.ClientId, config.ClientSecret, config.TenantId);
            string token = await auth.AcquireTokenForClientAsync();
            Assert.IsNotNull(token, "Security token must not be null.");
        }
    }
}
