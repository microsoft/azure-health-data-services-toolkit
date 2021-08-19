using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Fhir.Proxy.Bindings;
using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Configuration;
using Microsoft.Fhir.Proxy.Filters;
using Microsoft.Fhir.Proxy.Pipelines;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class PipelineTests
    {
        [TestCleanup]
        public async Task CleanupTest()
        {
            FilterFactory.Clear();
            ChannelFactory.Clear();
            PipelineFactory.Clear();
            await Task.CompletedTask;
        }

        [TestMethod]
        public void PipelineFactory_Register_Test()
        {
            BasicPipeline pipeline = new();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
            Assert.AreEqual(pipeline.Name, names[0], "Pipeline name mismatch.");
        }

        [TestMethod]
        public void PipelineFactory_GetNames_Test()
        {
            BasicPipeline pipeline = new();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
        }

        [TestMethod]
        public void PipelineFactory_Clear_Test()
        {
            BasicPipeline pipeline = new();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
            PipelineFactory.Clear();
            Assert.IsTrue(PipelineFactory.GetNames().Length == 0, "Expected no names in pipeline factory.");
        }

        [TestMethod]
        public void PipelineFactory_Create_Test()
        {
            BasicPipeline pipeline = new();
            PipelineFactory.Register(pipeline);
            Pipeline actual = PipelineFactory.Create(pipeline.Name);
            Assert.AreEqual(pipeline, actual, "Not expected type.");
        }

        [TestMethod]
        public void PipelineFactory_CreateWithSettings_Test()
        {
            PipelineSettings settings = PipelineSettings.Default;
            BasicPipeline pipeline = new();
            settings.Name = pipeline.Name;
            PipelineFactory.Register(pipeline);
            Pipeline actual = PipelineFactory.Create(settings);
            Assert.AreEqual(pipeline, actual, "Not expected type.");
        }

        [TestMethod]
        public async Task BasicPipeline_Test()
        {
            string requestUriString = "http://example.org/path";
            FilterCollection filters = new();
            ChannelCollection channels = new();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());
            BasicPipeline pipeline = new(filters, channels);
            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new(request);
            OperationContext output = await pipeline.ExecuteAsync(context);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.AreEqual(context.Request.Method, output.Request.Method, "method mismatch.");
            Assert.AreEqual(context.Request.RequestUri.ToString(), output.Request.RequestUri.ToString(), "Uri mismatch.");
        }

        [TestMethod]
        public async Task Pipeline_WithFilterError_Test()
        {
            string name = "ErrorFilter";
            bool fatal = true;
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string body = "stuff";
            var filter = new FakeFilterWithError(name, fatal, error, code, body);

            string requestUriString = "http://example.org/path";
            FilterCollection filters = new();
            ChannelCollection channels = new();
            filters.Add(filter);
            BasicPipeline pipeline = new(filters, channels);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new(request);
            _ = await pipeline.ExecuteAsync(context);
            Assert.IsTrue(complete, "Not complete");
        }

        [TestMethod]
        public async Task Pipeline_WithChannelError_Test()
        {
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            var channel = new FakeChannelWithError(error);

            string requestUriString = "http://example.org/path";
            FilterCollection filters = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            BasicPipeline pipeline = new(filters, channels);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new(request);
            _ = await pipeline.ExecuteAsync(context);
            Assert.IsTrue(complete, "Not complete");
        }


        [TestMethod]
        public void PipelineBuilder_Test()
        {
            PipelineSettings settings = PipelineSettings.Default;
            BasicPipeline pipeline = new();
            settings.Name = pipeline.Name;
            PipelineFactory.Register(pipeline);
            PipelineBuilder builder = new(settings);
            Pipeline builtPipeline = builder.Build();
            Assert.AreEqual(pipeline.Name, builtPipeline.Name, "Name mismatch.");
        }


        [TestMethod]
        public async Task WebPipelineManager_NoContent_Test()
        {
            FilterFactory.Register("Fake", typeof(FakeFilter));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            WebPipelineManager manager = new(input, new CoupledPipelineBinding(), output);

            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            var response = await manager.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task WebPipelineManager_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            FilterFactory.Register("Fake", typeof(FakeFilter));
            FilterFactory.Register("FakeFilterWithContent", typeof(FakeFilterWithContent));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake", "FakeFilterWithContent" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            WebPipelineManager manager = new(input, new CoupledPipelineBinding(), output);

            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            var response = await manager.ExecuteAsync(request);
            string actualContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WebPipelineManager_ForcedError_Test()
        {
            FilterFactory.Register("FakeBoom", typeof(FakeFilter));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            WebPipelineManager manager = new(input, new CoupledPipelineBinding(), output);

            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            var response = await manager.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");

        }

        [TestMethod]
        public async Task FunctionPipelineManager_NoContent_Test()
        {
            FilterFactory.Register("Fake", typeof(FakeFilter));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            AzureFunctionPipelineManager manager = new(input, new CoupledPipelineBinding(), output);

            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            var response = await manager.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task FunctionPipelineManager_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            FilterFactory.Register("Fake", typeof(FakeFilter));
            FilterFactory.Register("FakeFilterWithContent", typeof(FakeFilterWithContent));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake", "FakeFilterWithContent" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            AzureFunctionPipelineManager manager = new(input, new CoupledPipelineBinding(), output);

            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            var response = await manager.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content.Length, response.Body.Length, "Content length mismatch.");
        }

        [TestMethod]
        public async Task FunctionPipelineManager_ForcedError_Test()
        {
            FilterFactory.Register("FakeBoom", typeof(FakeFilter));
            ChannelFactory.Register("FakeChannel", typeof(FakeChannel), null);

            PipelineSettings input = new("BasicPipeline", new string[] { "Fake" }, new string[] { "FakeChannel" });
            PipelineSettings output = input;
            PipelineFactory.Register(new BasicPipeline());
            AzureFunctionPipelineManager manager = new(input, new CoupledPipelineBinding(), output);
            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);

            var response = await manager.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");
        }

    }
}
