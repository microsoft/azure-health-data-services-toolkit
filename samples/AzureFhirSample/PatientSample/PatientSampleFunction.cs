using Azure.Health.DataServices.Pipelines;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PatientSample
{
    public class PatientSampleFunction
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestData, HttpResponseData> pipeline;

        public PatientSampleFunction(IPipeline<HttpRequestData, HttpResponseData> pipeline, ILoggerFactory loggerFactory)
        {
            this.pipeline = pipeline;
            _logger = loggerFactory.CreateLogger<PatientSampleFunction>();
        }


        [Function("PatientSampleFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Patient/{id}/$everything")] HttpRequestData req)
        {
            _logger.LogInformation("Patient sample pipeline started...");
            var responseData = await pipeline.ExecuteAsync(req);
            responseData.Headers.Remove("Content-length");
            return responseData;
        }

    }
}
