using System.Reflection;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using UseCaseSample.Configuration;
using UseCaseSample.Filters;

namespace QuickstartSample.Tests
{
    [TestClass]
    public class UseCaseSampleTests
    {
        private static MyServiceConfig? config;
        private static ILogger<UseCaseSampleFilter>? filterLogger;

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

            filterLogger = loggerFactory.CreateLogger<UseCaseSampleFilter>();
        }

        [TestMethod]
        public async Task UseCaseFilter_Get_Test()
        {
            string json = await File.ReadAllTextAsync("../../../CapabilityStatement.json");

            OperationContext filterContext = new();
            filterContext.Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/metadata");
            filterContext.ContentString = json;

            UseCaseSampleFilter filter = new(null, filterLogger!);
            OperationContext resultContext = await filter.ExecuteAsync(filterContext);

            JObject jobj = JObject.Parse(resultContext.ContentString);
            var objExists = jobj.ContainsKey("instantiates");
            Assert.IsTrue(objExists);
            }
    }
}
