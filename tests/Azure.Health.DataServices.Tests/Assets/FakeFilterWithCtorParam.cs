using System;
using System.Threading.Tasks;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Pipelines;

namespace Azure.Health.DataServices.Tests.Assets
{
    public class FakeFilterWithCtorParam : IFilter
    {
        public FakeFilterWithCtorParam(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;
        public string Id { get; private set; }

        public string Name { get; private set; }

        public StatusType ExecutionStatusType => StatusType.Any;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, false));
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
