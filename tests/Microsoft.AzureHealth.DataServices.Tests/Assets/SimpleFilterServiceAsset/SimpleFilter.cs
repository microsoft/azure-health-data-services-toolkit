using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilter : IInputFilter
    {
        public SimpleFilter(IOptions<SimpleFilterOptions> options, IHttpCustomHeaderCollection customHeaders)
        {
            id = Guid.NewGuid().ToString();
            baseUrl = options.Value.BaseUrl;
            method = new HttpMethod(options.Value.HttpMethod);
            path = options.Value.Path;
            this.customerHeaders = customHeaders;
        }

        private readonly string baseUrl;
        private readonly HttpMethod method;
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
            NameValueCollection nvc = customerHeaders.RequestAppendAndReplace(context.Request);
            TestMessage msg = new() { Value = "filter" };
            string json = JsonConvert.SerializeObject(msg);
            HttpRequestMessageBuilder builder = new (method, baseUrl, path, headers: nvc, content: Encoding.UTF8.GetBytes(json));
            HttpClient client = new();
            HttpResponseMessage response = await client.SendAsync(builder.Build());
            context.StatusCode = response.StatusCode;
            context.ContentString = await response.Content.ReadAsStringAsync();

            return context;
        }
    }
}
