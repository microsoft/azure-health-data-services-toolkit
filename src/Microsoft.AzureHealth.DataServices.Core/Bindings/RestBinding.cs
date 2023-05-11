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

        /// <summary>
        /// Creates an instance of RestBinding.
        /// </summary>
        /// <param name="options">Rest binding options.</param>
        /// <param name="client">HttpClient used to exeucute the binding</param>
        /// <param name="logger">Optional logger.</param>
        public RestBinding(IOptions<RestBindingOptions> options, HttpClient client, ILogger<RestBinding> logger = null)
        {
            this.options = options;
            this.client = client;
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly IOptions<RestBindingOptions> options;
        private readonly HttpClient client;
        private readonly ILogger logger;


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
                NameValueCollection headers = context.Headers.RequestAppendAndReplace(context.Request, false);

                // Forward the token if required via configuration.
                string? token = options.Value.PassThroughAuthorizationHeader ? context.Request.Headers.Authorization?.Parameter?.ToString() : null;

                string contentType = context.Request.Content?.Headers?.ContentType?.MediaType?.ToString() ?? "application/json";

                HttpRequestMessageBuilder builder = new(method: context.Request.Method,
                                                    baseUrl: options.Value.BaseAddress,
                                                    path: context.Request.RequestUri.LocalPath,
                                                    query: context.Request.RequestUri.Query,
                                                    headers: headers,
                                                    content: context.Request.Content == null ? null : await context.Request.Content.ReadAsByteArrayAsync(),
                                                    securityToken: token,
                                                    contentType: contentType);

                HttpRequestMessage request = builder.Build();
                var resp = await client.SendAsync(request);

                context.StatusCode = resp.StatusCode;
                context.Content = await resp.Content?.ReadAsByteArrayAsync();

                if (options.Value.AddResponseHeaders)
                {
                    context.Headers.UpdateFromResponse(resp);
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
