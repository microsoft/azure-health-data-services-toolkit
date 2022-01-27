using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Clients;
using Microsoft.Health.Fhir.Proxy.Clients.Headers;
using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleFilter : IInputFilter
    {
        public SimpleFilter(IOptions<SimpleFilterOptions> options, IHttpCustomHeaderCollection customHeaders, IHttpCustomIdentityHeaderCollection identityHeaders)
        {
            id = Guid.NewGuid().ToString();
            baseUrl = options.Value.BaseUrl;
            method = options.Value.HttpMethod;
            path = options.Value.Path;
            this.customerHeaders = customHeaders;
            this.identityHeaders = identityHeaders;
        }

        private readonly string baseUrl;
        private readonly string method;
        private readonly string path;
        private readonly IHttpCustomHeaderCollection customerHeaders;
        private readonly IHttpCustomIdentityHeaderCollection identityHeaders;

        private readonly string id;
        public string Id => id;

        public string Name => "SimpleFilter";

        public StatusType ExecutionStatusType => StatusType.Any;

#pragma warning disable CS0067 // The event 'SimpleFilter.OnFilterError' is never used
        public event EventHandler<FilterErrorEventArgs> OnFilterError;
#pragma warning restore CS0067 // The event 'SimpleFilter.OnFilterError' is never used

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            NameValueCollection nvc = context.Request.GetHeaders();
            nvc = customerHeaders.AppendHeaders(nvc);
            nvc = identityHeaders.AppendCustomHeaders(context.Request, nvc);
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
