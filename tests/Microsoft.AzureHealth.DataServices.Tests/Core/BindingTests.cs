using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureHealth.DataServices.Tests.Core
{
    [TestClass]
    public class BindingTests
    {
        private static HttpEchoListener listener;
        private static readonly int port = 1888;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            listener = new();
            listener.StartAsync(port).GetAwaiter();
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            listener.StopAsync().GetAwaiter();
        }

        [TestMethod]
        public void RestPipelineBinding_Properties_Test()
        {
            string name = "RestBinding";
            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions());

            IBinding binding = new RestBinding(options, new HttpClient());

            Assert.AreEqual(name, binding.Name, "Binding name mismatch.");
            Assert.IsNotNull(binding.Id, "Expected not null Id");
        }

        [TestMethod]
        public async Task RestPipelineBinding_Error_Test()
        {
            OperationContext context = null;
            Exception error = null;
            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions());

            IBinding binding = new RestBinding(options, new HttpClient());
            binding.OnError += (i, args) =>
            {
                error = args.Error;
            };

            _ = await binding.ExecuteAsync(context);
            Assert.IsNotNull(error, "Expected error.");
        }

        [TestMethod]
        public async Task RestPipelineBinding_TokenCredential_Test()
        {
            Uri baseAddress = new Uri($"http://localhost:{port}");
            string uriString = $"http://localhost:{port}?name=value";
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            request.Headers.Add("TestHeader", "TestValue");
            OperationContext context = new()
            {
                Request = request,
            };

            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                BaseAddress = baseAddress,
                AddResponseHeaders = true,
            });

            // Generate a HTTPClient with a BearerTokenHandler
            FakeTokenCredential credential = new();
            string tokenValue = Guid.NewGuid().ToString();
            credential.TokenFactory = (x, y) => new AccessToken(tokenValue, DateTimeOffset.UtcNow.AddMinutes(10));
            HttpClient client = GenerateClient(baseAddress, _ => new BearerTokenHandler(credential, baseAddress, null));
           
            IBinding binding = new RestBinding(options, client);

            OperationContext actualContext = await binding.ExecuteAsync(context);

            // Echo API sends request headers back so we can look for Authorization header
            Assert.IsTrue(actualContext.Headers.Where(x => x.Name == "Authorization" && x.Value == $"Bearer {tokenValue}" && x.HeaderType == CustomHeaderType.ResponseStatic).Count() == 1);
        }

        [TestMethod]
        public async Task RestPipelineBinding_Complete_Test()
        {
            Uri baseAddress = new Uri($"http://localhost:{port}");
            string uriString = $"http://localhost:{port}?name=value";
            string expectedContext = "{ \"name\": \"value\" }";
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");
            request.Headers.Add("TestHeader", "TestValue");
            OperationContext context = new()
            {
                Request = request,
            };

            // Testing with
            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                BaseAddress = new Uri(uriString),
                Credential = new FakeTokenCredential(),
                PassThroughAuthorizationHeader = true,
                AddResponseHeaders = true,
            });

            // Generate a HTTPClient with a BearerTokenHandler
            FakeTokenCredential credential = new();
            string tokenValue = Guid.NewGuid().ToString();
            credential.TokenFactory = (x, y) => new AccessToken(tokenValue, DateTimeOffset.UtcNow.AddMinutes(10));
            HttpClient client = GenerateClient(baseAddress, _ => new BearerTokenHandler(credential, baseAddress, null));

            IBinding binding = new RestBinding(options, client);
            string argId = null;
            string argBindingName = null;
            OperationContext argContext = null;

            binding.OnComplete += (i, args) =>
            {
                argId = args.Id;
                argBindingName = args.Name;
                argContext = args.Context;
            };

            binding.OnError += (i, args) =>
            {
                Assert.Fail("Error not expected.");
            };

            OperationContext actualContext = await binding.ExecuteAsync(context);
            string actualResult = actualContext.ContentString;
            Assert.AreEqual(argId, binding.Id, "Id mismatch.");
            Assert.AreEqual(argBindingName, binding.Name, "Name mismatch.");
            Assert.AreEqual(argContext.Request.Method, actualContext.Request.Method, "Method mismatch.");
            Assert.AreEqual(argContext.Request.RequestUri.ToString(), actualContext.Request.RequestUri.ToString(), "Request URI mismatch.");

            // Echo API sends request headers back so we can look for Authorization header
            Assert.IsTrue(actualContext.Headers.Where(x => x.Name == "Authorization" && x.Value == "Bearer token" && x.HeaderType == CustomHeaderType.ResponseStatic).Count() == 1);
            Assert.IsTrue(actualContext.Headers.Where(x => x.Name == "TestHeader" && x.Value == "TestValue" && x.HeaderType == CustomHeaderType.ResponseStatic).Count() == 1);
            Assert.AreEqual(expectedContext, actualResult, "Content mismatch.");
        }

        private HttpClient GenerateClient(Uri baseAddress, Func<IServiceProvider, DelegatingHandler>? messageHandler)
        {
            // Create a new service collection
            var services = new ServiceCollection();

            // Add HttpClientFactory to the service collection
            var httpBuilder = services.AddHttpClient("TestClient");

            if (messageHandler is not null)
            {
                httpBuilder.AddHttpMessageHandler(messageHandler);
            }            

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Get an instance of IHttpClientFactory
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            var client = httpClientFactory.CreateClient("TestClient");
            client.BaseAddress = baseAddress;
            return client; 
        }
    }
}
