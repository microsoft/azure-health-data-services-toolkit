using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quickstart;
using Quickstart.Filters;
using Serilog;
using System.Collections.Specialized;
using System.Net;

namespace QuickstartSample.Tests
{
    public class TestFactory
    {
        private static ILoggerFactory? loggerFactoryInstance;

        public static ILoggerFactory TestLoggerFactory
        {
            get
            {
                if (loggerFactoryInstance is null)
                {
                    loggerFactoryInstance = CreateTestLoggerFactory();
                }

                return loggerFactoryInstance;
            }
        }

        public static QuickstartFunction CreateQuickstartFunction()
        {
            IInputFilterCollection filters = new InputFilterCollection
            {
                new QuickstartFilter(null, TestLoggerFactory.CreateLogger<QuickstartFilter>())
            };

            AzureFunctionPipeline pipeline = new(filters);

            return new QuickstartFunction(pipeline, TestLoggerFactory);
        }

        public static Mock<HttpRequestData> CreateMockedHttpRequest(string method, NameValueCollection query, string body, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILoggerFactory>(TestLoggerFactory);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new Mock<FunctionContext>();
            context.SetupProperty(c => c.InstanceServices, serviceProvider);

            var request = new Mock<HttpRequestData>(context.Object);

            request.Setup(req => req.Query).Returns(query);

            request.Setup(req => req.Headers).Returns(new HttpHeadersCollection(headers));

            request.Setup(req => req.Method).Returns(method);

            var response = new Mock<HttpResponseData>(context.Object);
            response.Setup(resp => resp.Headers).Returns(new HttpHeadersCollection());
            response.Setup(req => req.Body).Returns(new MemoryStream());

            request.Setup(req => req.CreateResponse(It.IsAny<HttpStatusCode>())).Returns(response.Object);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(body);
            writer.Flush();
            stream.Position = 0;
            request.Setup(req => req.Body).Returns(stream);
            return request;
        }

        public static ILoggerFactory CreateTestLoggerFactory()
        {
            string logPath = "../../quickstartlog.txt";

        var slog = new LoggerConfiguration()
            .WriteTo.File(
            logPath,
            shared: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(10000))
            .MinimumLevel.Debug()
            .CreateLogger();

            ILoggerFactory factory = LoggerFactory.Create(log =>
            {
                log.SetMinimumLevel(LogLevel.Information);
                log.AddConsole();
                log.AddSerilog(slog);
            });

            return factory;
        }
    }
}
