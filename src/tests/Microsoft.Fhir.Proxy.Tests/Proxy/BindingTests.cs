using Microsoft.Fhir.Proxy.Bindings;
using Microsoft.Fhir.Proxy.Pipelines;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
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
        public void CoupledPipelineBinding_Properties_Test()
        {
            string name = "CoupledPipelineBinding";
            PipelineBinding binding = new CoupledPipelineBinding();

            Assert.AreEqual(name, binding.Name, "Binding name mismatch.");
            Assert.IsNotNull(binding.Id, "Expected not null Id");
        }

        [TestMethod]
        public async Task CouplePipelineBinding_Error_Test()
        {
            OperationContext context = null;
            Exception error = null;
            PipelineBinding binding = new CoupledPipelineBinding();
            binding.OnError += (i, args) =>
            {
                error = args.Error;
            };

            _ = await binding.ExecuteAsync(context);
            Assert.IsNotNull(error, "Expected error.");
        }

        [TestMethod]
        public async Task CouplePipelineBinding_Complete_Test()
        {
            string uriString = "https://www.example.org/path?name=value";
            OperationContext context = new()
            {
                Request = new HttpRequestMessage(HttpMethod.Get, uriString)
            };

            PipelineBinding binding = new CoupledPipelineBinding();
            string argId = null;
            string argBindingName = null;
            OperationContext argContext = null;

            binding.OnComplete += (i, args) =>
            {
                argId = args.Id;
                argBindingName = args.Name;
                argContext = args.Context;
            };

            OperationContext actualContext = await binding.ExecuteAsync(context);
            Assert.AreEqual(argId, binding.Id, "Id mismatch.");
            Assert.AreEqual(argBindingName, binding.Name, "Name mismatch.");
            Assert.AreEqual(argContext.Request.Method, actualContext.Request.Method, "Method mismatch.");
            Assert.AreEqual(argContext.Request.RequestUri.ToString(), actualContext.Request.RequestUri.ToString(), "Request URI mismatch.");
        }

        [TestMethod]
        public void FhirPipelineBinding_Properties_Test()
        {
            string name = "FhirPipelineBinding";
            PipelineBinding binding = new FhirPipelineBinding();

            Assert.AreEqual(name, binding.Name, "Binding name mismatch.");
            Assert.IsNotNull(binding.Id, "Expected not null Id");
        }

        [TestMethod]
        public async Task FhirPipelineBinding_Error_Test()
        {
            OperationContext context = null;
            Exception error = null;
            PipelineBinding binding = new FhirPipelineBinding();
            binding.OnError += (i, args) =>
            {
                error = args.Error;
            };

            _ = await binding.ExecuteAsync(context);
            Assert.IsNotNull(error, "Expected error.");
        }

        [TestMethod]
        public async Task FhirPipelineBinding_Complete_Test()
        {
            string uriString = $"http://localhost:{port}?name=value";
            string expectedContext = "{ \"name\": \"value\" }";
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");
            OperationContext context = new()
            {
                Request = request,
            };

            PipelineBinding binding = new FhirPipelineBinding();
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
