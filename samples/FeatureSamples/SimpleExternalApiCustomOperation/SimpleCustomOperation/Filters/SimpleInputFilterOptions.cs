
using Azure.Health.DataServices.Pipelines;

namespace SimpleCustomOperation.Filters
{
    public class SimpleInputFilterOptions
    {
        public string HttpMethod { get; set; }
        public string BaseUrl { get; set; }
        public string Path { get; set; }

        public StatusType ExecutionStatus { get; set; }
    }
}
