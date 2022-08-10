using DataServices.Pipelines;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PatientEverything
{
    public class PatientEverythingFunction
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestData, HttpResponseData> pipeline;

        public PatientEverythingFunction(IPipeline<HttpRequestData, HttpResponseData> pipeline, ILoggerFactory loggerFactory)
        {
            this.pipeline = pipeline;
            _logger = loggerFactory.CreateLogger<PatientEverythingFunction>();
        }

        [Function("PatientEverythingFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Patient/{id}/$everything")] HttpRequestData req)
        {
            _logger.LogInformation("Patient everything pipeline started...");
            return await pipeline.ExecuteAsync(req);
        }
    }
}
