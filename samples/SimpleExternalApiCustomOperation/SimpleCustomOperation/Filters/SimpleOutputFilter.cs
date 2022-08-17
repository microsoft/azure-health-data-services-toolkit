using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Pipelines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;

namespace SimpleCustomOperation.Filters
{
    public class SimpleOutputFilter : IOutputFilter
    {
        public SimpleOutputFilter(IOptions<SimpleOutputFilterOptions> options, ILogger<SimpleOutputFilter> logger = null)
        {
            status = options.Value.ExecutionStatus;
            this.logger = logger;
            id = Guid.NewGuid().ToString();
        }

        private readonly StatusType status;
        private readonly string id;
        private readonly ILogger logger;
        public string Id => id;

        public string Name => "SimpleOutputFilter";

        public StatusType ExecutionStatusType => status;

#pragma warning disable CS0067 // The event 'SimpleOutputFilter.OnFilterError' is never used
        public event EventHandler<FilterErrorEventArgs> OnFilterError;
#pragma warning restore CS0067 // The event 'SimpleOutputFilter.OnFilterError' is never used

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            logger?.LogInformation("Entered {Name}", Name);
            var output = JsonConvert.DeserializeObject<Message>(context.ContentString);
            output.Value = $"{output.Value}-{Name}";
            context.StatusCode = System.Net.HttpStatusCode.OK;
            context.ContentString = JsonConvert.SerializeObject(output);
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
