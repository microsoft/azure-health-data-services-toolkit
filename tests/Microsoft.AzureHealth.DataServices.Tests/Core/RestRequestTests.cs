using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureHealth.DataServices.Tests.Core
{
    [TestClass]
    public class RestRequestTests
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
        public void RequestBuilder_Validate_RequestProperties_Test()
        {
            HttpMethod method = HttpMethod.Post;
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = "https://www.example.org";
            string path = "/path";
            string query = "key=value";
            string jsonType = "application/json";
            NameValueCollection headers = new()
            {
                { "Accept", jsonType },
                { "Authorize", "Bearer foo" },
                { "Content-Type", "application/xml" },
                { "Location", "kitchen" }
            };

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl), path, query, headers, content);
            HttpRequestMessage actual = builder.Build();


            Assert.AreEqual(method, actual.Method, "Method mismatch.");
            Assert.AreEqual(content.Length, actual.Content.Headers.ContentLength, "Content length mismatch.");
            Assert.AreEqual(jsonType, actual.Headers.Accept.ToString(), "Accept mismatch.");
            Assert.AreEqual(jsonType, actual.Content.Headers.ContentType.ToString(), "Content type mismatch.");
            Assert.AreEqual($"{baseUrl}{path}?{query}", actual.RequestUri.ToString().ToLowerInvariant(), "Url mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Post_Test()
        {
            HttpMethod method = HttpMethod.Post;
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl), path, query, headers, content, jsonType);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await msg.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Get_Test()
        {
            HttpMethod method = HttpMethod.Get;
            string contentString = "{ \"key\": \"value\" }";
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "key=value";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl), path, query, headers, null, jsonType);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await msg.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Put_Test()
        {
            HttpMethod method = HttpMethod.Put;
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl), path, query, headers, content, jsonType);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Delete_Test()
        {
            HttpMethod method = HttpMethod.Delete;
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "id=1";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl), path, query, headers, null, jsonType);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Patch_Test()
        {
            HttpMethod method = HttpMethod.Patch;
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            HttpRequestMessageBuilder builder = new(method, new Uri(baseUrl),  path, query, headers, content, jsonType);
            HttpClient client = new();
            HttpResponseMessage msg = await client.SendAsync(builder.Build());
            Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
        }
    }
}
