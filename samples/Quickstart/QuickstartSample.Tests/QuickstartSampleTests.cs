using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Json;
using Azure.Health.DataServices.Json.Transforms;
using Azure.Health.DataServices.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Quickstart.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;

namespace QuickstartSample.Tests
{
    [TestClass]
    public class QuickstartSampleTests
    {
        private static MyServiceConfig config;
        private static TestContext testContext;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            testContext = context;
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("AZURE_");
            IConfigurationRoot root = builder.Build();
            config = new MyServiceConfig();
            root.Bind(config);

        }

        [TestMethod]
        public async Task QuckStartSample_Post()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");
            try
            {
                IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new());
                Authenticator auth = new(options);
                string securityToken = await auth.AcquireTokenForClientAsync(config.FhirServerUrl);

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");
                byte[] postContent = Encoding.UTF8.GetBytes(json);
                RestRequestBuilder builder = new("post", config.FhirServerUrl, securityToken, "Patient", null, customHeader, postContent);
                RestRequest req = new(builder);
                HttpResponseMessage msg = await req.SendAsync();
                var content = await msg.Content.ReadAsStringAsync();
                HttpStatusCode statusCode = HttpStatusCode.Created;
                Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
            }
            catch (Exception ex)
            {
                testContext.WriteLine(ex.StackTrace);
            }
        }

        [TestMethod]
        public async Task QuckStartSample_Get()
        {
            try
            {
                var path = "Patient";
                string Id = "f49a176a-1029-4f9a-a5a7-ca87e957e7df";
                IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new());
                Authenticator auth = new(options);
                string securityToken = await auth.AcquireTokenForClientAsync(config.FhirServerUrl);

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");

                RestRequestBuilder builder = new("Get", config.FhirServerUrl, securityToken, path, $"_id={Id}", customHeader, null);
                RestRequest req = new(builder);
                HttpResponseMessage msg = await req.SendAsync();
                var content = await msg.Content.ReadAsStringAsync();
                Assert.IsNotNull(content, "Content is null.");
                Assert.IsFalse(string.IsNullOrEmpty(content), "Content is empty.");
                testContext.WriteLine(content);
                JToken jsonToken = JToken.Parse(content);
                Assert.IsNotNull(jsonToken, "Content is null");
                Assert.AreEqual("Bundle", jsonToken.SelectToken("$.resourceType").Value<string>(), "Bundle not found.");
            }
            catch (Exception ex)
            {
                testContext.WriteLine(ex.StackTrace);
            }
        }


        [TestMethod]
        public async Task QuckStartSample_Put()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");
            try
            {
                var path = "Patient";
                string Id = "f49a176a-1029-4f9a-a5a7-ca87e957e7df";
                IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new());
                Authenticator auth = new(options);
                string securityToken = await auth.AcquireTokenForClientAsync(config.FhirServerUrl);

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");
                string transformedJson = TransformJson(json, Id);
                byte[] postContent = Encoding.UTF8.GetBytes(transformedJson);
                RestRequestBuilder builder = new("put", config.FhirServerUrl, securityToken, path, $"_id={Id}", customHeader, postContent);
                RestRequest req = new(builder);
                HttpResponseMessage msg = await req.SendAsync();
                var content = await msg.Content.ReadAsStringAsync();
                HttpStatusCode statusCode = HttpStatusCode.OK;
                Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public async Task QuckStartSample_Delete()
        {
            try
            {
                var path = "Patient";
                string Id = "f49a176a-1029-4f9a-a5a7-ca87e957e7df";
                IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new());
                Authenticator auth = new(options);
                string securityToken = await auth.AcquireTokenForClientAsync(config.FhirServerUrl);

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");

                RestRequestBuilder builder = new("Delete", config.FhirServerUrl, securityToken, path, $"_id={Id}", customHeader, null);
                RestRequest req = new(builder);
                HttpResponseMessage msg = await req.SendAsync();
                HttpStatusCode statusCode = HttpStatusCode.NoContent;
                Assert.AreEqual(statusCode, msg.StatusCode, "Status code mismatch.");
            }
            catch (Exception ex)
            {
                testContext.WriteLine(ex.StackTrace);
            }
        }

        private string TransformJson(string reqContent, string patientId)
        {
            string json = reqContent;
            JObject jobj = JObject.Parse(json);
            TransformCollection transforms = new();
            if (!jobj.Exists("$.id"))
            {
                AddTransform addTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{ \"id\": \"" + patientId + "\" }",
                };
                transforms.Add(addTrans);
            }

            if (!jobj.Exists("$.language"))
            {
                AddTransform addTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{ \"language\": \"en\" }",
                };
                transforms.Add(addTrans);
            }
            if (!jobj.Exists("$.meta.security"))
            {
                AddTransform addMetaTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{\"meta\":{\"security\":[{\"system\":\"http://terminology.hl7.org/CodeSystem/v3-ActReason\",\"code\":\"HTEST\",\"display\":\"test health data\"}]}}"
                };
                transforms.Add(addMetaTrans);
            }


            TransformPolicy policy = new(transforms);
            string transformedJson = policy.Transform(json);
            return transformedJson;
        }

    }
}
