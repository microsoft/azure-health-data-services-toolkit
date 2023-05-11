using Microsoft.AzureHealth.DataServices.Pipelines;
using System;
using System.Net.Http;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilterOptions
    {
        public Uri BaseUrl { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public string Path { get; set; }

        public StatusType ExecutionStatus { get; set; }
    }
}
