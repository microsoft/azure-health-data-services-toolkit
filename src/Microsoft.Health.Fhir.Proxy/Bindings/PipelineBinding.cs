using Microsoft.Health.Fhir.Proxy.Pipelines;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// Abstract binding used to couple pipelines or act as pipeline termination.
    /// </summary>
    public abstract class PipelineBinding
    {
        /// <summary>
        /// Gets the name of the binding.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the unique id on the binding instance.
        /// </summary>
        public abstract string Id { get; internal set; }

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        public abstract event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        public abstract event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes binding.
        /// </summary>
        /// <param name="context">Operation context.</param>
        /// <returns>Operation context.</returns>
        public abstract Task<OperationContext> ExecuteAsync(OperationContext context);
    }
}
