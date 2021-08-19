using Microsoft.Fhir.Proxy.Filters;
using Microsoft.Fhir.Proxy.Pipelines;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Assets
{
    public class FakeFilterWithError : IFilter
    {
        public FakeFilterWithError(string name, bool fatal, Exception error, HttpStatusCode code, string body)
        {
            Name = name;
            this.fatal = fatal;
            this.error = error;
            this.code = code;
            this.body = body;
            Id = Guid.NewGuid().ToString();
        }

        private readonly bool fatal;
        private readonly Exception error;
        private readonly HttpStatusCode code;
        private readonly string body;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;
        public string Id { get; private set; }

        public string Name { get; private set; }

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, fatal, error, code, body));
            return await Task.FromResult<OperationContext>(null);
        }
    }
}
