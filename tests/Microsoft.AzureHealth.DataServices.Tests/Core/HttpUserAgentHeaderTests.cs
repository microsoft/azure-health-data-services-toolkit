using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.AzureHealth.DataServices.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Core
{
    [TestClass]
    public class HttpUserAgentHeaderTests
    {
        private static HttpTestListener listener;
        private static readonly int port = 1239;
        private static string userAgentHeader = "User-Agent";
        private static string expectedHeader = "Output-Header";
        private static string expectedHeaderValue = "my-injected-value";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            listener = new();
            List<Tuple<string, string>> headerMap = new List<Tuple<string, string>>();
            headerMap.Add(new Tuple<string, string>(userAgentHeader, expectedHeader));

            listener = new();
            listener.StartAsync(port, headerMap).GetAwaiter();
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            listener.StopAsync().GetAwaiter();
        }

        [TestMethod]
        public void RestRequest_BuildDefaultUserAgentHeader_Test()
        {
            RestRequestBuilder builder = new("GET", "http://localhost", "", "foo", null, null, null, "application/json");
            var request = builder.Build();
            Assert.AreEqual(request.Headers.UserAgent.ToString(), RestRequestBuilder.DefaultUserAgentHeader);
        }

        [TestMethod]
        public async Task RestRequest_CustomUserAgentHeader_Test()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(userAgentHeader, "fake-agent");
            IHeaderNameValuePair customHeader = new HeaderNameValuePair(userAgentHeader, expectedHeaderValue, CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] customHeaders = new IHeaderNameValuePair[] { customHeader };
            IHttpCustomHeaderCollection collection = new HttpCustomHeaderCollection(customHeaders);
            var headers = collection.RequestAppendAndReplace(request);


            RestRequestBuilder builder = new("GET", $"http://localhost:{port}", "", null, null, headers, null, "application/json");
            RestRequest req = new RestRequest(builder);
            var response = await req.SendAsync();
            string actualContent = await response.Content.ReadAsStringAsync();
            //actualContent = actualContent.Replace("{  }", "");
            var token = JToken.Parse(actualContent);
            var propToken = token.GetToken($"$.{expectedHeader}");
            var actual = propToken.GetValue<string>();
            Assert.AreEqual(actual, expectedHeaderValue);
        }
    }
}
