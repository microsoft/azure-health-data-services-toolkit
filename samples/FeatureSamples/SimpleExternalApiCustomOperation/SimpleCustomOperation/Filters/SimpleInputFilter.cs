using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;

namespace SimpleCustomOperation.Filters
{
    public class SimpleInputFilter : IInputFilter
    {
        private readonly Uri _baseUrl;
        private readonly HttpMethod _method;
        private readonly string _path;
        private readonly StatusType _status;
        private readonly string _id;
        private readonly ILogger _logger;

        public SimpleInputFilter(IOptions<SimpleInputFilterOptions> options, ILogger<SimpleInputFilter> logger = null)
        {
            _baseUrl = options.Value.BaseUrl;
            _method = options.Value.HttpMethod;
            _path = options.Value.Path;
            _status = options.Value.ExecutionStatus;
            _logger = logger;
            _id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id => _id;

        public string Name => "SimpleInputFilter";

        public StatusType ExecutionStatusType => _status;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            try
            {
                _logger?.LogInformation("Entered {Name}", Name);
                System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(context.Request.RequestUri.Query);

                // create a message to send to the binding
                Message msg = new() { Value = $"{nvc[0]}-{Name}" };
                byte[] content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

                // replace the request context with the new message;
                context.Request.Content = new ByteArrayContent(content);

                // update the request uri to send to the server in the sample
                context.UpdateRequestUri(_method, _baseUrl.GetLeftPart(UriPartial.Authority), _path, null);

                // set the request content type to send to the server in the sample
                context.Request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                // the context will be used to call the RestBinding in the sample.
                return await Task.FromResult<OperationContext>(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in {Name}", Name);
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true));
                return context;
            }
        }
    }
}
