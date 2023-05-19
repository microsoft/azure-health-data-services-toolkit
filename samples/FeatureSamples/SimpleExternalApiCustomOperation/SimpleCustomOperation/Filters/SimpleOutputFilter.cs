using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;

namespace SimpleCustomOperation.Filters
{
    public class SimpleOutputFilter : IOutputFilter
    {
        private readonly StatusType _status;
        private readonly string _id;
        private readonly ILogger _logger;

        public SimpleOutputFilter(IOptions<SimpleOutputFilterOptions> options, ILogger<SimpleOutputFilter> logger = null)
        {
            _status = options.Value.ExecutionStatus;
            _logger = logger;
            _id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id => _id;

        public string Name => "SimpleOutputFilter";

        public StatusType ExecutionStatusType => _status;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            try
            {
                _logger?.LogInformation("Entered {Name}", Name);
                Message output = JsonConvert.DeserializeObject<Message>(context.ContentString);
                output.Value = $"{output.Value}-{Name}";
                context.StatusCode = System.Net.HttpStatusCode.OK;
                context.ContentString = JsonConvert.SerializeObject(output);
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
