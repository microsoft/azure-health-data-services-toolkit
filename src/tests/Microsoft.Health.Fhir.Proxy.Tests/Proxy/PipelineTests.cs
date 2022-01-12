using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Bindings;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class PipelineTests
    {
        

        [TestMethod]
        public async Task WebPipeline_Simple_Test()
        {
            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, filters, channels);
           
            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
        }

        [TestMethod]
        public async Task WebPipeline_WithFilterError_Test()
        {
            string name = "ErrorFilter";
            bool fatal = true;
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string body = "stuff";
            var filter = new FakeFilterWithError(name, fatal, error, code, body);

            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, filters, null, null, null, null, null, null);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Error was expected");
        }

        [TestMethod]
        public async Task WebPipeline_WithChannelErrorIgnored_Test()
        {
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            var channel = new FakeChannelWithError(error);

            string requestUriString = "http://example.org/path";
            IInputChannelCollection channels = new InputChannelCollection
            {
                channel
            };
            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, null, channels);
            bool trigger = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                trigger = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(trigger, "Error was not expected");
        }

        [TestMethod]
        public async Task WebPipeline_WithChannelErrorOmitted_Test()
        {
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            var channel = new FakeChannelWithError(error);

            string requestUriString = "http://example.org/path";
            IInputChannelCollection channels = new InputChannelCollection
            {
                channel
            };
            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = false,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, null, channels);
            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            pipeline.OnError += (a, args) =>
            {
                Assert.Fail("Unexpected error.");
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Fail to complete.");
        }


       


        [TestMethod]
        public async Task WebPipeline_NoContent_Test()
        {
            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            IBinding binding = new CoupledBinding();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, filters, channels, binding);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
;
            HttpResponseMessage response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task WebPipeline_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            channels.Add(new FakeChannel());

            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            
            HttpResponseMessage response = await pipeline.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.IsTrue(complete, "Pipeline not complete.");
            string actualContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WebPipeline_ForcedError_Test()
        {
            FaultFilter filter = new();
            bool faulted = false;
            filter.OnFilterError += (a, args) =>
            {
                faulted = true;
            };
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(options, filters);

            

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            bool fault = false;
            pipeline.OnError += (a, args) =>
            {
                fault = true;
            };

            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            var response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(faulted, "Not faulted.");
            Assert.IsFalse(complete, "Should not be complete.");
            Assert.IsTrue(fault, "Should have fault.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");



        }

        [TestMethod]
        public async Task FunctionPipeline_NoContent_Test()
        {
            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            IBinding binding = new CoupledBinding();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(options, filters, channels, binding);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            
            HttpResponseData response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task FunctionPipelineManager_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            channels.Add(new FakeChannel());

            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(options, filters, channels);

            //bool complete = false;
            //pipeline.OnComplete += (a, args) =>
            //{
            //    complete = true;
            //};

            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            var response = await pipeline.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content.Length, response.Body.Length, "Content length mismatch.");


        }

        [TestMethod]
        public async Task FunctionPipelineManager_ForcedError_Test()
        {
            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);

            IInputFilterCollection filters = new InputFilterCollection
            {
                new FaultFilter()
            };
            IOptions<PipelineOptions> options = Options.Create<PipelineOptions>(new PipelineOptions()
            {
                FaultOnChannelError = true,
            });
            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(options, filters);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            bool fault = false;
            pipeline.OnError += (a, args) =>
            {
                fault = true;
            };
            
            var response = await pipeline.ExecuteAsync(request);
            Assert.IsFalse(complete, "Should not be complete.");
            Assert.IsTrue(fault, "Should have fault.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");
        }

    }
}
