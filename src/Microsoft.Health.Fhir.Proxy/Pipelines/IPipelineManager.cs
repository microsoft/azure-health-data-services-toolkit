using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Interface for pipeline manager.
    /// </summary>
    /// <typeparam name="TRequest">Type of request for input into the pipeline.</typeparam>
    /// <typeparam name="TResponse">Type of response from output the pipeline.</typeparam>
    public interface IPipelineManager<TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest input);
    }
}
