using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// A binding used to call the server and couples input/output pipelines or acts as terminator for an input pipeline.
    /// </summary>
    public class RestBinding : IBinding
    {

        /// <summary>
        /// Creates an instance of RestBinding.
        /// </summary>
        /// <param name="genericRestClient">Generic Rest Client.</param>
        /// <param name="logger">Optional logger.</param>
        public RestBinding(IAzureClientFactory<GenericRestClient> genericRestClient, ILogger<RestBinding> logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
            this.genericRestClient = genericRestClient;
        }

        private readonly ILogger logger;
        private readonly IAzureClientFactory<GenericRestClient> genericRestClient;


        /// <summary>
        /// Gets the name of the binding "RestBinding".
        /// </summary>
        public string Name => "RestBinding";

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
            logger?.LogInformation("{Name}-{Id} binding received.", Name, Id);

            if (context == null)
            {
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            try
            {                
                GenericRestClient client = genericRestClient.CreateClient("Default");
                var resp = await client.SendAsync(context);
                context.StatusCode = (HttpStatusCode)resp.Status;
                context.Content = resp.Content.ToArray();

                //if (options.Value.AddResponseHeaders)
                //{
                //    context.Headers.UpdateFromResponse(resp);
                //}

                OnComplete?.Invoke(this, new BindingCompleteEventArgs(Id, Name, context));
                logger?.LogInformation("{Name}-{Id} completed.", Name, Id);
                return context;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{Name}-{Id} fault with server request.", Name, Id);
                context.IsFatal = true;
                context.Error = ex;
                context.Content = null;
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, ex));
                logger?.LogInformation("{Name}-{Id} signaled error.", Name, Id);
                return context;
            }
        }

    }
}
