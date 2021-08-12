using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Pipelines;
using System;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Bindings
{
    /// <summary>
    /// Binding that does no work, but couples an input to output pipeline or acts as a terminator for an input pipeline.
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

        public override string Name => "CoupledPipelineBinding";

        public override string Id { get; internal set; }

        public override event EventHandler<PipelineErrorEventArgs> OnError;

        public override event EventHandler<PipelineCompleteEventArgs> OnComplete;

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
