using Castle.Core.Logging;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Quickstart.Configuration;
using Quickstart.Filters;
using System.Net;
using System.Reflection;

namespace QuickstartSample.Tests
{
    [TestClass]
    public class QuickstartSampleTests
    {
        private static MyServiceConfig? config;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            // Setup configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("AZURE_");
            IConfigurationRoot root = builder.Build();
            config = new MyServiceConfig();
            root.Bind(config);
        }

        [TestMethod]
        public async Task QuickstartFilter_Post_Test()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");

            OperationContext filterContext= new();
            filterContext.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            filterContext.ContentString = json;

            ILogger<QuickstartFilter> logger = TestFactory.TestLoggerFactory.CreateLogger<QuickstartFilter>();
            QuickstartFilter filter = new(null, logger);
            var resultContext = await filter.ExecuteAsync(filterContext);

            JObject jobj = JObject.Parse(resultContext.ContentString);

            Assert.AreEqual("en", jobj["language"]!);
            Assert.AreEqual("HTEST", jobj["meta"]!["security"]![0]!["code"]!);
        }
    }
}
