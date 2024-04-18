using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

namespace UseCaseSample
{
    /// <summary>
    /// Azure function class for Quickststart toolkit sample
    /// </summary>
    public class UseCaseSampleFunction
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestData, HttpResponseData> pipeline;

        public UseCaseSampleFunction(IPipeline<HttpRequestData, HttpResponseData> pipeline, ILoggerFactory loggerFactory)
        {
            this.pipeline = pipeline;
            _logger = loggerFactory.CreateLogger<UseCaseSampleFunction>();
        }

        [Function("UseCaseSample")]
        public async Task<HttpResponseData> UseCaseSample([HttpTrigger(AuthorizationLevel.Function, Route = "{*all}")] HttpRequestData req)
        {
            // This is what hooks up the Azure Function to the Custom Operation pipeline
            _logger.LogInformation("UseCaseSample sample pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }
    }
}
