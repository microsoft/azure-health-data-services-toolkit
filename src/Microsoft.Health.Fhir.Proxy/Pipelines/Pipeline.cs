using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Abstact pipeline.
    /// </summary>
    public abstract class Pipeline : IDisposable
    {
        /// <summary>
        /// An event that signals an exception in the pipeline.
        /// </summary>
        public abstract event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals the pipeline has completed.
        /// </summary>
        public abstract event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Gets the instance ID of the pipeline.
        /// </summary>
        public virtual string Id { get; internal set; }

        /// <summary>
        /// Gets the name of the pipeline.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets a collection of channels used by the pipeline.
        /// </summary>
        public virtual ChannelCollection Channels { get; internal set; }

        /// <summary>
        /// Gets a collection of filters used by the pipeline.
        /// </summary>
        public virtual FilterCollection Filters { get; internal set; }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <param name="context">The operation context to execute.</param>
        /// <returns>OperationContext</returns>
        public abstract Task<OperationContext> ExecuteAsync(OperationContext context);

        /// <summary>
        /// Disposes the pipeline.
        /// </summary>
        public abstract void Dispose();
    }
}
