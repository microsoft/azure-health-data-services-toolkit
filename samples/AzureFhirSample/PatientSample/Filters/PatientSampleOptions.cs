
using Azure.Health.DataServices.Pipelines;

namespace PatientSample.Filters
{
    public class PatientSampleOptions
    {
        public string FhirServerUrl { get; set; }

        public int PageSize { get; set; }

        public int MaxSize { get; set; }

        public double RetryDelaySeconds { get; set; }

        public int MaxRetryAttempts { get; set; }

        public StatusType ExecutionStatusType { get; set; }

    }
}
