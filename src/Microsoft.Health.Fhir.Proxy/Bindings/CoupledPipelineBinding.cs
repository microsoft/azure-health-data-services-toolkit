using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// A binding that does no work, but couples an input to output pipeline or acts as a terminator for an input pipeline.
    /// </summary>
    public class CoupledPipelineBinding : PipelineBinding
    {
        /// <summary>
        /// Creates an instance of the CoupledPipelineBinding.
        /// </summary>
        /// <param name="logger"></param>
        public CoupledPipelineBinding(ILogger logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly ILogger logger;

        /// <summary>
        /// Gets the name of the binding "CoupledPipelineBinding".
        /// </summary>
        public override string Name => "CoupledPipelineBinding";

        /// <summary>
        /// Gets a unique ID of the binding instance.
        /// </summary>
        public override string Id { get; internal set; }

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        public override event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals the binding has completed.
        /// </summary>
        public override event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes the binding.
        /// </summary>
        /// <param name="context">Operation context.</param>
        /// <returns>Operation context.</returns>
        public override async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            if (context == null)
            {
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            logger?.LogInformation($"{Name}-{Id} received.");
            OnComplete?.Invoke(this, new PipelineCompleteEventArgs(Id, Name, context));
            logger?.LogInformation($"{Name}-{Id} completed.");
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
