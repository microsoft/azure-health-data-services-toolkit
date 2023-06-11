using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Json;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.Headers
{
    [TestClass]
    public class HttpUserAgentHeaderTests
    {
        private static readonly string UserAgentHeader = "User-Agent";
        private static readonly string ExpectedHeader = "Output-Header";
        private static readonly string CustomTestHeaderValue = "my-injected-value";
        private static readonly int Port = 1239;
        private static HttpTestListener listener;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            listener = new();
            List<Tuple<string, string>> headerMap = new()
            {
                new Tuple<string, string>(UserAgentHeader, ExpectedHeader),
            };

            listener = new();
            listener.StartAsync(Port, headerMap).GetAwaiter();
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            listener.StopAsync().GetAwaiter();
        }

        [TestMethod]
        public void RestRequest_BuildDefaultUserAgentHeader_Test()
        {
            HttpRequestMessageBuilder builder = new(HttpMethod.Get, new Uri("http://localhost"), string.Empty, "foo", null, null, "application/json");
            HttpRequestMessage request = builder.Build();
            Assert.AreEqual(request.Headers.UserAgent.ToString(), HttpRequestMessageBuilder.DefaultUserAgentHeader.ToString());
        }

        [TestMethod]
        public async Task RestRequest_CustomUserAgentHeader_Test()
        {
            HttpRequestMessage request = new();
            request.Headers.Add(UserAgentHeader, "fake-agent");
            IHeaderNameValuePair customHeader = new HeaderNameValuePair(UserAgentHeader, CustomTestHeaderValue, CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] customHeaders = new IHeaderNameValuePair[] { customHeader };
            IHttpCustomHeaderCollection collection = new HttpCustomHeaderCollection(customHeaders);
            System.Collections.Specialized.NameValueCollection headers = collection.RequestAppendAndReplace(request);

            HttpRequestMessageBuilder builder = new(HttpMethod.Get, new Uri($"http://localhost:{Port}"), headers: headers);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            string actualContent = await msg.Content.ReadAsStringAsync();

            // actualContent = actualContent.Replace("{  }", "");
            var token = JToken.Parse(actualContent);
            JToken propToken = token.GetToken($"$.{ExpectedHeader}");
            var actual = propToken.GetValue<string>();
            Assert.IsTrue(actual.Contains(HttpRequestMessageBuilder.DefaultUserAgentHeader.ToString()));
        }
    }
}
