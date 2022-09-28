using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.AzureHealth.DataServices.Tests.Proxy
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
            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                ServerUrl = "",
            });

            IOptions<ServiceIdentityOptions> soptions = Options.Create<ServiceIdentityOptions>(new ServiceIdentityOptions()
            {
                CredentialType = ClientCredentialType.ManagedIdentity,
            });

            IAuthenticator authenticator = new Authenticator(soptions);

            IBinding binding = new RestBinding(options, authenticator);

            Assert.AreEqual(name, binding.Name, "Binding name mismatch.");
            Assert.IsNotNull(binding.Id, "Expected not null Id");
        }

        [TestMethod]
        public async Task RestPipelineBinding_Error_Test()
        {
            OperationContext context = null;
            Exception error = null;
            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                ServerUrl = "",
            });

            IOptions<ServiceIdentityOptions> soptions = Options.Create<ServiceIdentityOptions>(new ServiceIdentityOptions()
            {
                CredentialType = ClientCredentialType.ManagedIdentity,
            });

            IAuthenticator authenticator = new Authenticator(soptions);

            IBinding binding = new RestBinding(options, authenticator);
            binding.OnError += (i, args) =>
            {
                error = args.Error;
            };

            _ = await binding.ExecuteAsync(context);
            Assert.IsNotNull(error, "Expected error.");
        }

        [TestMethod]
        public async Task RestPipelineBinding_Complete_Test()
        {
            string uriString = $"http://localhost:{port}?name=value";
            string expectedContext = "{ \"name\": \"value\" }";
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");
            OperationContext context = new()
            {
                Request = request,
            };

            IOptions<RestBindingOptions> options = Options.Create<RestBindingOptions>(new RestBindingOptions()
            {
                ServerUrl = uriString,
            });

            IOptions<ServiceIdentityOptions> soptions = Options.Create<ServiceIdentityOptions>(new ServiceIdentityOptions()
            {
                CredentialType = ClientCredentialType.ManagedIdentity,
            });

            var authenticator = new Mock<IAuthenticator>();
            authenticator.Setup(p => p.AcquireTokenForClientAsync(It.IsAny<string>(),
                                                                It.IsAny<string[]>(),
                                                                It.IsAny<string>(),
                                                                It.IsAny<string>(),
                                                                It.IsAny<string>(),
                                                                CancellationToken.None)).Returns(Task.FromResult<string>("token"));



            IBinding binding = new RestBinding(options, authenticator.Object);
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
            Assert.AreEqual(expectedContext, actualResult, "Content mismatch.");
        }
    }
}
