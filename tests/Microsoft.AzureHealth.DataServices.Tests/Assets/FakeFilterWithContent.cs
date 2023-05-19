using System;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FakeFilterWithContent : IFilter
    {
        private string id;

        public FakeFilterWithContent()
        {
            Id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id
        {
            get
            {
                return id;
            }

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

        public string Name => "FakeFilterWithContent";

        public StatusType ExecutionStatusType => StatusType.Any;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            string content = "{ \"property\": \"value\" }";
            context.ContentString = content;
            context.Headers.Add(new HeaderNameValuePair("TestName", "TestValue", CustomHeaderType.ResponseStatic));
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
