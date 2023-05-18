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
        private readonly Uri _baseUrl;
        private readonly HttpMethod _method;
        private readonly string _path;
        private readonly IHttpCustomHeaderCollection _customerHeaders;
        private readonly string _id;

        public SimpleFilter(IOptions<SimpleFilterOptions> options, IHttpCustomHeaderCollection customHeaders)
        {
            _id = Guid.NewGuid().ToString();
            _baseUrl = options.Value.BaseUrl;
            _method = options.Value.HttpMethod;
            _path = options.Value.Path;
            _customerHeaders = customHeaders;
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id => _id;

        public string Name => "SimpleFilter";

        public StatusType ExecutionStatusType => StatusType.Any;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            try
            {
                NameValueCollection nvc = _customerHeaders.RequestAppendAndReplace(context.Request);
                TestMessage msg = new() { Value = "filter" };
                string json = JsonConvert.SerializeObject(msg);
                HttpRequestMessageBuilder builder = new(_method, _baseUrl, _path, headers: nvc, content: Encoding.UTF8.GetBytes(json));
                HttpClient client = new();
                HttpResponseMessage response = await client.SendAsync(builder.Build());
                context.StatusCode = response.StatusCode;
                context.ContentString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, false, ex));
            }

            return context;
        }
    }
}
