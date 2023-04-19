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
        public async Task<HttpResponseData> Patient([HttpTrigger(AuthorizationLevel.Function, "get", "put", "delete", "patch", Route = "Patient/{id}")] HttpRequestData req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }


        [Function("SearchPatient")]
        public async Task<HttpResponseData> SearchPatient([HttpTrigger(AuthorizationLevel.Function, "get", Route = "{respurcetype}")] HttpRequestData req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }


        [Function("PostBundle")]
        public async Task<HttpResponseData> PostBundle([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
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
