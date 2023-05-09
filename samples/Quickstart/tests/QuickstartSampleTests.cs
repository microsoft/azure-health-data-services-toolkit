using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Json;
using Microsoft.AzureHealth.DataServices.Json.Transforms;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;
using Newtonsoft.Json.Linq;
using Quickstart.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace QuickstartSample.Tests
{
    public class QuickstartSampleTests
    {
        private static MyServiceConfig? config;
        private static ITestOutputHelper? testContext;

        public QuickstartSampleTests(ITestOutputHelper context)
        {
            testContext = context;
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("AZURE_");
            IConfigurationRoot root = builder.Build();
            config = new MyServiceConfig();
            root.Bind(config);
        }

        [Fact]
        public async Task QuckStartSample_Post()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");
            try
            {
                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");
                byte[] postContent = Encoding.UTF8.GetBytes(json);
                HttpRequestMessageBuilder builder = new(HttpMethod.Post, config!.FhirServerUrl, "Patient", null, customHeader, postContent);
                HttpClient client = new();
                HttpResponseMessage msg = await client.SendAsync(builder.Build());
                var content = await msg.Content.ReadAsStringAsync();
                HttpStatusCode statusCode = HttpStatusCode.Created;
                Assert.Equal(statusCode, msg.StatusCode);
            }
            catch (Exception ex)
            {
                testContext!.WriteLine(ex.StackTrace);
            }
        }

        [Fact]
        public async Task QuckStartSample_Get()
        {
            try
            {
                var path = "Patient";
                string Id = "85e2e069-e2e5-48ba-9b22-4af8653a9b17";

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");

                HttpRequestMessageBuilder builder = new(HttpMethod.Get, config!.FhirServerUrl, path, $"_id={Id}", customHeader, null);
                HttpClient client = new();
                HttpResponseMessage msg = await client.SendAsync(builder.Build());
                var content = await msg.Content.ReadAsStringAsync();
                Assert.NotNull(content);
                Assert.False(string.IsNullOrEmpty(content));
                testContext!.WriteLine(content);
                JToken jsonToken = JToken.Parse(content);
                Assert.NotNull(jsonToken);
                Assert.Equal("Bundle", jsonToken.SelectToken("$.resourceType")!.Value<string>());
            }
            catch (Exception ex)
            {
                testContext!.WriteLine(ex.StackTrace);
            }
        }


        [Fact]
        public async Task QuckStartSample_Put()
        {
            string json = await File.ReadAllTextAsync("../../../patient.json");
            try
            {
                var path = "Patient";
                string Id = "f49a176a-1029-4f9a-a5a7-ca87e957e7df";

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");
                string transformedJson = TransformJson(json, Id);
                byte[] postContent = Encoding.UTF8.GetBytes(transformedJson);
                HttpRequestMessageBuilder builder = new(HttpMethod.Put, config!.FhirServerUrl, path, $"_id={Id}", customHeader, postContent);
                HttpClient client = new();
                HttpResponseMessage msg = await client.SendAsync(builder.Build());
                var content = await msg.Content.ReadAsStringAsync();
                HttpStatusCode statusCode = HttpStatusCode.OK;
                Assert.Equal(statusCode, msg.StatusCode);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Fact]
        public async Task QuckStartSample_Delete()
        {
            try
            {
                var path = "Patient";
                string Id = "f49a176a-1029-4f9a-a5a7-ca87e957e7df";

                NameValueCollection customHeader = new NameValueCollection();
                customHeader.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");

                HttpRequestMessageBuilder builder = new(HttpMethod.Delete, config!.FhirServerUrl, path, $"_id={Id}", customHeader, null);
                HttpClient client = new();
                HttpResponseMessage msg = await client.SendAsync(builder.Build());
                HttpStatusCode statusCode = HttpStatusCode.NoContent;
                Assert.Equal(statusCode, msg.StatusCode);
            }
            catch (Exception ex)
            {
                testContext!.WriteLine(ex.StackTrace);
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
