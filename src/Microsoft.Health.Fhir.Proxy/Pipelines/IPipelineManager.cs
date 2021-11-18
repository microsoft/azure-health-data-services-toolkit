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
        /// <summary>
        /// Executes request and returns a response.
        /// </summary>
        /// <param name="input">Input request.</param>
        /// <returns>Output response.</returns>
        Task<TResponse> ExecuteAsync(TRequest input);
    }
}
