using Azure.Health.DataServices.Pipelines;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Quickstart
{
    public class QuickstartFunction
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestData, HttpResponseData> pipeline;

        public QuickstartFunction(IPipeline<HttpRequestData, HttpResponseData> pipeline, ILoggerFactory loggerFactory)
        {
            this.pipeline = pipeline;
            _logger = loggerFactory.CreateLogger<QuickstartFunction>();
        }

        [Function("QuickstartFunction")]
        public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "Patient/{id?}")] HttpRequestData req)
        {
            _logger.LogInformation("Patient sample pipeline started...");
            var responseData = await pipeline.ExecuteAsync(req);
            responseData.Headers.Remove("Content-length");
            return responseData;
        }
    }
}
