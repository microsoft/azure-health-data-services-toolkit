using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

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

        [Function("Patient")]
        public async Task<HttpResponseData> Patient([HttpTrigger(AuthorizationLevel.Function, "get", "put", "delete", Route = "Patient/{id}")] HttpRequestData req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }
        [Function("PatientPost")]
        public async Task<HttpResponseData> PatientPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Patient")] HttpRequestData req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }
    }
}