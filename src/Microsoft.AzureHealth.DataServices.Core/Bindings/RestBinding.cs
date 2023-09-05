using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// A binding used to call the server and couples input/output pipelines or acts as terminator for an input pipeline.
    /// </summary>
    public class RestBinding : IBinding
    {
        private readonly IOptions<RestBindingOptions> _options;
        private readonly HttpClient client;
        private readonly ILogger logger;

        /// <summary>
        /// Creates an instance of RestBinding.
        /// </summary>
        /// <param name="options">Rest binding options.</param>
        /// <param name="client">HttpClient used to exeucute the binding</param>
        /// <param name="logger">Optional logger.</param>
        public RestBinding(IOptions<RestBindingOptions> options, HttpClient client, ILogger<RestBinding> logger = null)
        {
            _options = options;
            this.client = client;
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// An event that signals an error in the binding.
        /// </summary>
        public event EventHandler<BindingErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals the binding has completed.
        /// </summary>
        public event EventHandler<BindingCompleteEventArgs> OnComplete;

        /// <summary>
        /// Gets the name of the binding "RestBinding".
        /// </summary>
        public string Name => "RestBinding";

        /// <summary>
        /// Gets a unique ID of the binding instance.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Executes the binding.
        /// </summary>
        /// <param name="context">Operation context.</param>
        /// <returns>Modified operation context by binding.</returns>
        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            logger?.LogInformation("{Name}-{Id} binding received.", Name, Id);

            if (context == null)
            {
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            // If the status code was set earlier in the pipeline, skip the binding.
            if (context.StatusCode > 0)
            {
                return context;
            }

            try
            {
                NameValueCollection headers = context.Headers.RequestAppendAndReplace(context.Request, true);

                // Forward the token if required via configuration.
                string token = _options.Value.PassThroughAuthorizationHeader ? context.Request.Headers.Authorization?.Parameter?.ToString() : null;

                string contentType = context.Request.Content?.Headers?.ContentType?.MediaType?.ToString() ?? "application/json";

                HttpRequestMessageBuilder builder = new(
                    method: context.Request.Method,
                    baseUrl: _options.Value.BaseAddress,
                    path: context.Request.RequestUri.LocalPath,
                    query: context.Request.RequestUri.Query,
                    headers: headers,
                    content: context.Content == null ? (context.Request.Content == null ? null : await context.Request.Content.ReadAsByteArrayAsync()) : context.Content,
                    securityToken: token,
                    contentType: contentType);

                HttpRequestMessage request = builder.Build();
                HttpResponseMessage resp = await client.SendAsync(request);

                context.StatusCode = resp.StatusCode;
                context.Content = await resp.Content?.ReadAsByteArrayAsync();

                if (_options.Value.AddResponseHeaders)
                {
                    context.Headers.UpdateFromResponse(resp, true);
                }

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
