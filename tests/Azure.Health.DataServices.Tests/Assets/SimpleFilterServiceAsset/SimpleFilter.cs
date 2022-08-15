using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azure.Health.DataServices.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilter : IInputFilter
    {
        public SimpleFilter(IOptions<SimpleFilterOptions> options, IHttpCustomHeaderCollection customHeaders)
        {
            id = Guid.NewGuid().ToString();
            baseUrl = options.Value.BaseUrl;
            method = options.Value.HttpMethod;
            path = options.Value.Path;
            this.customerHeaders = customHeaders;
        }

        private readonly string baseUrl;
        private readonly string method;
        private readonly string path;
        private readonly IHttpCustomHeaderCollection customerHeaders;

        private readonly string id;
        public string Id => id;

        public string Name => "SimpleFilter";

        public StatusType ExecutionStatusType => StatusType.Any;

#pragma warning disable CS0067 // The event 'SimpleFilter.OnFilterError' is never used
        public event EventHandler<FilterErrorEventArgs> OnFilterError;
#pragma warning restore CS0067 // The event 'SimpleFilter.OnFilterError' is never used

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            NameValueCollection nvc = customerHeaders.AppendAndReplace(context.Request);
            TestMessage msg = new() { Value = "filter" };
            string json = JsonConvert.SerializeObject(msg);
            RestRequestBuilder builder = new RestRequestBuilder(method, baseUrl, "", path, null, nvc, Encoding.UTF8.GetBytes(json), "application/json");
            RestRequest request = new(builder);
            HttpResponseMessage response = await request.SendAsync();
            context.StatusCode = response.StatusCode;
            context.ContentString = await response.Content.ReadAsStringAsync();

            return context;
        }
    }
}
