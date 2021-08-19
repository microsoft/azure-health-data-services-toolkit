using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Configuration;
using Microsoft.Fhir.Proxy.Filters;
using Microsoft.Fhir.Proxy.Pipelines;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class PipelineTests
    {
        [TestMethod]
        public void PipelineFactory_Register_Test()
        {
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
            Assert.AreEqual(pipeline.Name, names[0], "Pipeline name mismatch.");
        }

        [TestMethod]
        public void PipelineFactory_GetNames_Test()
        {
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
        }

        [TestMethod]
        public void PipelineFactory_Clear_Test()
        {
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            string[] names = PipelineFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Unexpected # of pipeline names.");
            PipelineFactory.Clear();
            Assert.IsTrue(PipelineFactory.GetNames().Length == 0, "Expected no names in pipeline factory.");
        }

        [TestMethod]
        public void PipelineFactory_Create_Test()
        {
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            Pipeline actual = PipelineFactory.Create(pipeline.Name);
            Assert.AreEqual(pipeline, actual, "Not expected type.");
        }

        [TestMethod]
        public void PipelineFactory_CreateWithSettings_Test()
        {
            PipelineSettings settings = PipelineSettings.Default;
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            Pipeline actual = PipelineFactory.Create(pipeline.Name, settings);
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
            BasicPipeline pipeline = new BasicPipeline(filters, channels);
            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new OperationContext(request);
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
            BasicPipeline pipeline = new BasicPipeline(filters, channels);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new OperationContext(request);
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
            BasicPipeline pipeline = new BasicPipeline(filters, channels);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            OperationContext context = new OperationContext(request);
            _ = await pipeline.ExecuteAsync(context);
            Assert.IsTrue(complete, "Not complete");
        }


        [TestMethod]
        public void PipelineBuilder_Test()
        {
            PipelineSettings settings = PipelineSettings.Default;
            BasicPipeline pipeline = new BasicPipeline();
            PipelineFactory.Register(pipeline);
            PipelineBuilder builder = new(pipeline.Name, settings);
            Pipeline builtPipeline = builder.Build();
            Assert.AreEqual(pipeline.Name, builtPipeline.Name, "Name mismatch.");
        }

    }
}
