using DataServices.Pipelines;

namespace DataServices.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilterOptions
    {
        public string BaseUrl { get; set; }

        public string HttpMethod { get; set; }

        public string Path { get; set; }

        public StatusType ExecutionStatus { get; set; }
    }
}
