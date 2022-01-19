using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// A binding that does no work, but couples an input to output pipeline or acts as a terminator for an input pipeline.
    /// </summary>
    public class CoupledBinding : IBinding
    {
        /// <summary>
        /// Creates an instance of the CoupledPipelineBinding.
        /// </summary>
        /// <param name="logger"></param>
        public CoupledBinding(ILogger<CoupledBinding> logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly ILogger logger;

        /// <summary>
        /// Gets the name of the binding "CoupledBinding".
        /// </summary>
        public string Name => "CoupledBinding";

        /// <summary>
        /// Gets a unique ID of the binding instance.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        public event EventHandler<BindingErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals the binding has completed.
        /// </summary>
        public event EventHandler<BindingCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes the binding.
        /// </summary>
        /// <param name="context">Operation context.</param>
        /// <returns>Operation context.</returns>
        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            if (context == null)
            {
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            logger?.LogInformation("{Name}-{Id} received.", Name, Id);
            OnComplete?.Invoke(this, new BindingCompleteEventArgs(Id, Name, context));
            logger?.LogInformation("{Name}-{Id} completed.", Name, Id);
            return await Task.FromResult<OperationContext>(context);
        }
    }
}
