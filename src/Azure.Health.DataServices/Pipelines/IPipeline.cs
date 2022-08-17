using System;
using System.Threading.Tasks;

namespace Azure.Health.DataServices.Pipelines
{
    /// <summary>
    /// An interface for a pipeline.
    /// </summary>
    /// <typeparam name="TRequest">Type of request received by the pipeline.</typeparam>
    /// <typeparam name="TResponse">Type of response emitted by the pipeline.</typeparam>
    public interface IPipeline<TRequest, TResponse>
    {
        /// <summary>
        /// Gets the name for the pipeline.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique identifier of the pipeline instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Event that signals an error in the pipeline.
        /// </summary>
        event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// Event that signals the pipeline as completed.
        /// </summary>
        event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <param name="request">Request to start to the pipeline.</param>
        /// <returns>Response message to be sent to caller.</returns>
        Task<TResponse> ExecuteAsync(TRequest request);
    }
}
