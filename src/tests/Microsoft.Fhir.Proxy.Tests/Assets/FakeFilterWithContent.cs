using Microsoft.Fhir.Proxy.Filters;
using Microsoft.Fhir.Proxy.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Assets
{
    public class FakeFilterWithContent : IFilter
    {
        public FakeFilterWithContent()
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


        public string Name => "FakeFilterWithContent";

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            string content = "{ \"property\": \"value\" }";
            context.ContentString = content;
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
