using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureHealth.DataServices.Tests.Headers
{
    [TestClass]
    public class HeaderConversionTests
    {
        private static readonly int Port = 1240;
        private static HttpTestListener listener;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            listener = new();
            listener.StartAsync(Port).GetAwaiter();
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            listener.StopAsync().GetAwaiter();
        }

        [TestMethod]
        public async Task HttpMessageExtensions_ContentTypeConversionSimple_Test()
        {
            IHttpCustomHeaderCollection headers = new HttpCustomHeaderCollection();
            IInputFilterCollection filters = new InputFilterCollection();
            headers.Add(new HeaderNameValuePair("Content-Type", "application/json", CustomHeaderType.ResponseStatic));
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, headers: headers);

            HttpRequestMessage request = new(HttpMethod.Get, "http://example.org/path");
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.AreEqual("application/json", output.Content.Headers.GetValues("Content-Type").First());
        }

        [TestMethod]
        public async Task HttpMessageExtensions_ContentTypeConversionFhir_Test()
        {
            IHttpCustomHeaderCollection headers = new HttpCustomHeaderCollection();
            IInputFilterCollection filters = new InputFilterCollection();
            headers.Add(new HeaderNameValuePair("Content-Type", "application/fhir+json; charset=utf-8", CustomHeaderType.ResponseStatic));
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, headers: headers);

            HttpRequestMessage request = new(HttpMethod.Get, "http://example.org/path");
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.AreEqual("application/fhir+json; charset=utf-8", output.Content.Headers.GetValues("Content-Type").First());
        }

        [TestMethod]
        public async Task HttpMessageExtensions_ContentTypeBadInput_Test()
        {
            IHttpCustomHeaderCollection headers = new HttpCustomHeaderCollection();
            IInputFilterCollection filters = new InputFilterCollection();
            headers.Add(new HeaderNameValuePair("Content-Type", "=I-Am-Not-Real=", CustomHeaderType.ResponseStatic));
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, headers: headers);

            HttpRequestMessage request = new(HttpMethod.Get, "http://example.org/path");
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.AreEqual(0, output.Content.Headers.Count(x => x.Key == "Content-Type"));
        }

        [TestMethod]
        public async Task HttpMessageExtensions_ContentTypeNoInput_Test()
        {
            IHttpCustomHeaderCollection headers = new HttpCustomHeaderCollection();
            IInputFilterCollection filters = new InputFilterCollection
            {
                new FakeFilter(),
                new FakeFilterWithContent(),
            };
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, headers: headers);

            HttpRequestMessage request = new(HttpMethod.Get, "http://example.org/path");
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.AreEqual(0, output.Content.Headers.Count(x => x.Key == "Content-Type"));
        }
    }
}
