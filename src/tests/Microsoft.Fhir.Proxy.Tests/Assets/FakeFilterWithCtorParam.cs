using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets
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

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, false));
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
