using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public interface IPipelineManager<TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest input);
    }
}
