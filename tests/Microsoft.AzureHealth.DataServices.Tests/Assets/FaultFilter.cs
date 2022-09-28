using System;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FaultFilter : IFilter
    {
        public FaultFilter()
        {
            id = Guid.NewGuid().ToString();
        }

        private readonly string id;
        public string Id => id;

        public string Name => "FaultFilter";

        public StatusType ExecutionStatusType => StatusType.Any;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true));
            await Task.CompletedTask;
            return null;
        }
    }
}
