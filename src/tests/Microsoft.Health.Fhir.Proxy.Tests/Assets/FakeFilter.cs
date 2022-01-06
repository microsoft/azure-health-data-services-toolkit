using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets
{
    public class FakeFilter :  IFilter
    {
        public FakeFilter()
        {
            Id = Guid.NewGuid().ToString();
        }

        private string id;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;
        public string Id
        {
            get { return id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, "NA", true));
                }
                else
                {
                    id = value;
                }
            }
        }


        public string Name => "Fake";

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
