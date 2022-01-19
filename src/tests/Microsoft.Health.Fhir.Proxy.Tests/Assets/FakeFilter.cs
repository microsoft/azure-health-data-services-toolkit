using System;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets
{
    public class FakeFilter : IFilter
    {
        public FakeFilter(StatusType execStatus)
        {
            status = execStatus;
            Id = Guid.NewGuid().ToString();
        }

        public FakeFilter()
        {
            status = StatusType.Any;
            Id = Guid.NewGuid().ToString();
        }

        private string id;
        private readonly StatusType status;

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

        public StatusType ExecutionStatusType => status;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
