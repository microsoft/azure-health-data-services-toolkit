using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

namespace Quickstart
{
    /// <summary>
    /// Azure function class for Quickststart toolkit sample
    /// </summary>
    public class QuickstartFunction
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline;

        public QuickstartFunction(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILoggerFactory loggerFactory)
        {
            this.pipeline = pipeline;
            _logger = loggerFactory.CreateLogger<QuickstartFunction>();
        }

        [Function("Patient")]
        public async Task<HttpResponseMessage> Patient([HttpTrigger(AuthorizationLevel.Function, "get", "put", "delete", Route = "Patient/{id}")] HttpRequest req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req.ConvertToHttpRequestMessage());
        }

        [Function("PatientPost")]
        public async Task<HttpResponseMessage> PatientPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Patient")] HttpRequest req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("Patient sample pipeline started...");
            return await pipeline.ExecuteAsync(req.ConvertToHttpRequestMessage());
        }
    }
}
