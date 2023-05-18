using System;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FakeFilter : IFilter
    {
        private readonly StatusType _status;
        private string _id;

        public FakeFilter(StatusType execStatus)
        {
            _status = execStatus;
            Id = Guid.NewGuid().ToString();
        }

        public FakeFilter()
        {
            _status = StatusType.Any;
            Id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, "NA", true));
                }
                else
                {
                    _id = value;
                }
            }
        }

        public string Name => "Fake";

        public StatusType ExecutionStatusType => _status;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
