using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataServices.Clients;
using DataServices.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataServices.Tests.Proxy
{
    [TestClass]
    public class RetryTests
    {
        private int counter;
        private static HttpEchoListener listener;
        private static readonly int port = 7888;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine(context.TestName);
            listener = new();
            listener.StartAsync(port, 2, 4).GetAwaiter();
        }

        [ClassCleanup]
        public static async Task CleanupTestSuite()
        {
            await listener.StopAsync();
        }

        [TestMethod]
        public void Retry_Test()
        {
            var backOff = TimeSpan.FromSeconds(1.0);
            int maxAttempts = 4;

            async Task<int> func()
            {
                return await RetryFunc();
            }

            var output = Retry.Execute<int>(func, backOff, maxAttempts);
            int i = output.Result;
            Assert.AreEqual(maxAttempts - 1, i);
        }

        [TestMethod]
        public async Task Retry_RestRequest409_Test()
        {
            string method = "POST";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string token = "token";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            RestRequestBuilder builder = new(method, baseUrl, token, path, query, headers, content, jsonType);
            RestRequest request = new(builder);
            TimeSpan backOff = TimeSpan.FromSeconds(1.0);
            int attempts = 3;
            var response = await Retry.ExecuteRequest(request, backOff, attempts);
            Assert.AreEqual(statusCode, response.StatusCode, "Status code mismatch.");
        }

        private Task<int> RetryFunc()
        {
            counter++;
            if (counter < 3)
            {
                throw new Exception("Fail < 3.");
            }
            else
            {
                return Task.FromResult(counter);
            }
        }
    }
}
