using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Fhir.Proxy.Clients;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class RestRequestTests
    {
        private static HttpEchoListener listener;
        private static X509Certificate2 certificate;
        private static readonly int port = 1888;

        public RestRequestTests()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FHIR_SERVER_URL")))
            {
                var configuration = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Microsoft.Fhir.Proxy.Tests.Proxy.RestRequestTests).Assembly)
                    .Build();

                Environment.SetEnvironmentVariable("PROXY_CLIENT_ID", configuration.GetValue<string>("PROXY_CLIENT_ID"));
                Environment.SetEnvironmentVariable("PROXY_CLIENT_SECRET", configuration.GetValue<string>("PROXY_CLIENT_SECRET"));
                Environment.SetEnvironmentVariable("PROXY_TENANT_ID", configuration.GetValue<string>("PROXY_TENANT_ID"));
            }

            string clientId = Environment.GetEnvironmentVariable("PROXY_CLIENT_ID");
            string tenantId = Environment.GetEnvironmentVariable("PROXY_TENANT_ID");
            string secret = Environment.GetEnvironmentVariable("PROXY_CLIENT_SECRET");
            string keyVaultUriString = "https://fhir-proxy-ci-vault.vault.azure.net/";
            string certificationName = "localhost";
            ClientSecretCredential cred = new(tenantId, clientId, secret);
            CertificateClient client = new(new Uri(keyVaultUriString), cred);
            Response<KeyVaultCertificateWithPolicy> resp = client.GetCertificate(certificationName);
            certificate = new(resp.Value.Cer);
        }

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
            string method = "POST";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = "https://www.example.org";
            string path = "/path";
            string query = "key=value";
            string token = "token";
            string jsonType = "application/json";
            NameValueCollection headers = new();
            headers.Add("Accept", jsonType);
            headers.Add("Authorize", "Bearer foo");
            headers.Add("Content-Type", "application/xml");
            headers.Add("Location", "kitchen");

            RestRequestBuilder builder = new(method, baseUrl, token, path, query, headers, content);
            HttpWebRequest actual = builder.Build();

            Assert.AreEqual(method, actual.Method, "Method mismatch.");
            Assert.AreEqual(content.Length, actual.ContentLength, "Content length mismatch.");
            Assert.AreEqual(jsonType, actual.Accept, "Accept mismatch.");
            Assert.AreEqual(jsonType, actual.ContentType, "Content type mismatch.");
            Assert.AreEqual($"Bearer {token}", actual.Headers["Authorization"], "Authorization mismatch.");
            Assert.AreEqual($"{baseUrl}{path}?{query}", actual.RequestUri.ToString().ToLowerInvariant(), "Url mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Post_Test()
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
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await message.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Post_WithCert_Test()
        {
            string method = "POST";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            RestRequestBuilder builder = new(method, baseUrl, certificate, path, query, headers, content, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await message.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Get_Test()
        {
            string method = "GET";
            string contentString = "{ \"key\": \"value\" }";
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "key=value";
            string token = "token";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            RestRequestBuilder builder = new(method, baseUrl, token, path, query, headers, null, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await message.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Get_WithCert_Test()
        {
            string method = "GET";
            string contentString = "{ \"key\": \"value\" }";
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "key=value";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            RestRequestBuilder builder = new(method, baseUrl, certificate, path, query, headers, null, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
            Assert.AreEqual(contentString, await message.Content.ReadAsStringAsync(), "Content mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Put_Test()
        {
            string method = "PUT";
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
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Put_WithCert_Test()
        {
            string method = "PUT";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.OK;

            RestRequestBuilder builder = new(method, baseUrl, certificate, path, query, headers, content, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Delete_Test()
        {
            string method = "Delete";
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "id=1";
            string token = "token";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            RestRequestBuilder builder = new(method, baseUrl, token, path, query, headers, null, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Delete_WithCert_Test()
        {
            string method = "Delete";
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = "id=1";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            RestRequestBuilder builder = new(method, baseUrl, certificate, path, query, headers, null, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Patch_Test()
        {
            string method = "PATCH";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string token = "token";
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            RestRequestBuilder builder = new(method, baseUrl, token, path, query, headers, content, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }

        [TestMethod]
        public async Task RestRequest_Patch_WithCert_Test()
        {
            string method = "PATCH";
            string contentString = "{ \"prop\": \"value\"}";
            byte[] content = Encoding.UTF8.GetBytes(contentString);
            string baseUrl = $"http://localhost:{port}";
            string path = null;
            string query = null;
            string jsonType = "application/json";
            NameValueCollection headers = null;

            HttpStatusCode statusCode = HttpStatusCode.NoContent;

            RestRequestBuilder builder = new(method, baseUrl, certificate, path, query, headers, content, jsonType);
            RestRequest request = new(builder);
            HttpResponseMessage message = await request.SendAsync();
            Assert.AreEqual(statusCode, message.StatusCode, "Status code mismatch.");
        }
    }
}
