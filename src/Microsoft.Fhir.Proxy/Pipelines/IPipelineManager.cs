using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public interface IPipelineManager<TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest input);
    }
}
