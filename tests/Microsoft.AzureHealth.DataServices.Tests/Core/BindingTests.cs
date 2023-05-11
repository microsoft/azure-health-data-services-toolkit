using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            string uriString = $"http://localhost:{port}?name=value";
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            request.Headers.Add("TestHeader", "TestValue");
            OperationContext context = new()
            {
                Request = request,
            };

            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                BaseAddress = new Uri(uriString),
                AddResponseHeaders = true,
            });

            // Generate a HTTPClient with a BearerTokenHandler
            FakeTokenCredential credential = new();
            string tokenValue = Guid.NewGuid().ToString();
            credential.TokenFactory = (x, y) => new AccessToken(tokenValue, DateTimeOffset.UtcNow.AddMinutes(10));
            HttpClient client = HttpClientFactory.Create(new BearerTokenHandler(credential, new Uri(uriString), null));
            client.BaseAddress = new Uri(uriString);

            IBinding binding = new RestBinding(options, client);

            OperationContext actualContext = await binding.ExecuteAsync(context);

            // Echo API sends request headers back so we can look for Authorization header
            Assert.IsTrue(actualContext.Headers.Where(x => x.Name == "Authorization" && x.Value == $"Bearer {tokenValue}" && x.HeaderType == CustomHeaderType.ResponseStatic).Count() == 1);
        }

        [TestMethod]
        public async Task RestPipelineBinding_Complete_Test()
        { 
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

            // Generate a HTTPClient with a BearerTokenHandler. This handler should be ovveriden by Authorization header via PassThroughAuthorizationHeader.
            FakeTokenCredential credential = new();
            string tokenValue = Guid.NewGuid().ToString();
            credential.TokenFactory = (x, y) => new AccessToken(tokenValue, DateTimeOffset.UtcNow.AddMinutes(10));
            HttpClient client = HttpClientFactory.Create(new BearerTokenHandler(credential, new Uri(uriString), null));
            client.BaseAddress = new Uri(uriString);

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
    }
}
