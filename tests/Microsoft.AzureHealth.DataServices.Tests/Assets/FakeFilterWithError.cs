using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FakeFilterWithError : IFilter
    {
        private readonly bool _fatal;
        private readonly Exception _error;
        private readonly HttpStatusCode _code;
        private readonly string _body;

        public FakeFilterWithError(string name, bool fatal, Exception error, HttpStatusCode code, string body)
        {
            Name = name;
            _fatal = fatal;
            _error = error;
            _code = code;
            _body = body;
            Id = Guid.NewGuid().ToString();
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id { get; private set; }

        public string Name { get; private set; }

        public StatusType ExecutionStatusType => StatusType.Any;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, _fatal, _error, _code, _body));
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
