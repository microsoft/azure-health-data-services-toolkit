using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilterOptions
    {
        public string BaseUrl { get; set; }

        public string HttpMethod { get; set; }

        public string Path { get; set; }

        public StatusType ExecutionStatus { get; set; }
    }
}
