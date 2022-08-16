using DataServices.Filters;
using DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Shared;
using System.Net.Http.Headers;
using System.Text;

namespace SimpleCustomOperation.Filters
{
    public class SimpleInputFilter : IInputFilter
    {
        public SimpleInputFilter(IOptions<SimpleInputFilterOptions> options, ILogger<SimpleInputFilter> logger = null)
        {
            baseUrl = options.Value.BaseUrl;
            method = options.Value.HttpMethod;
            path = options.Value.Path;
            status = options.Value.ExecutionStatus;
            this.logger = logger;
            id = Guid.NewGuid().ToString();
        }

        private readonly string baseUrl;
        private readonly string method;
        private readonly string path;
        private readonly StatusType status;
        private readonly string id;
        private readonly ILogger logger;
        public string Id => id;

        public string Name => "SimpleInputFilter";

        public StatusType ExecutionStatusType => status;

#pragma warning disable CS0067 // The event 'SimpleInputFilter.OnFilterError' is never used
        public event EventHandler<FilterErrorEventArgs> OnFilterError;
#pragma warning restore CS0067 // The event 'SimpleInputFilter.OnFilterError' is never used

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            logger?.LogInformation("Entered {Name}", Name);
            var nvc = context.Request.RequestUri.ParseQueryString();

            //create a message to send to the binding
            Message msg = new() { Value = $"{nvc[0]}-{Name}" };
            byte[] content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

            //replace the request context with the new message;
            context.Request.Content = new ByteArrayContent(content);

            //update the request uri to send to the server in the sample
            HttpMethod httpMethod = new(method);
            context.UpdateRequestUri(httpMethod, baseUrl, path, null);

            //set the request content type to send to the server in the sample
            context.Request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //the context will be used to call the RestBinding in the sample.
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
