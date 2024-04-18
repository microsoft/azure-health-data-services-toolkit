using System.Reflection;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Quickstart.Configuration;
using Quickstart.Filters;

namespace QuickstartSample.Tests
{
    [TestClass]
    public class QuickstartSampleTests
    {
        private static MyServiceConfig? config;
        private static ILogger<QuickstartFilter>? filterLogger;

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

            using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());

            filterLogger = loggerFactory.CreateLogger<QuickstartFilter>();
        }

        [TestMethod]
        public async Task QuickstartFilter_Post_Test()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");

            OperationContext filterContext = new();
            filterContext.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            filterContext.ContentString = json;

            QuickstartFilter filter = new(null, filterLogger!);
            OperationContext resultContext = await filter.ExecuteAsync(filterContext);

            JObject jobj = JObject.Parse(resultContext.ContentString);

            Assert.AreEqual("en", jobj["communication"]![0]!["language"]!["coding"]![0]!["code"]!);
            Assert.AreEqual("HTEST", jobj["meta"]!["security"]![0]!["code"]!);
        }
    }
}
