
using Azure.Health.DataServices.Pipelines;

namespace Quickstart.Filters
{
    public class QuickstartOptions
    {
        public string FhirServerUrl { get; set; }

        public double RetryDelaySeconds { get; set; }

        public int MaxRetryAttempts { get; set; }

        public StatusType ExecutionStatusType { get; set; }

        public int PageSize { get; set; }

        public int MaxSize { get; set; }



    }
}
