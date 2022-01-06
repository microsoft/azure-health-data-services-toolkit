using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public interface IPipeline<TRequest, TResponse>
    {
        string Name { get; }

        string Id { get; }

        event EventHandler<PipelineErrorEventArgs> OnError;

        event EventHandler<PipelineCompleteEventArgs> OnComplete;

        Task<TResponse> ExecuteAsync(TRequest request);
    }
}
