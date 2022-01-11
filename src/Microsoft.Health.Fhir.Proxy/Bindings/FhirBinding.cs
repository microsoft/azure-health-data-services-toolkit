using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Clients;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using Microsoft.Health.Fhir.Proxy.Security;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Bindings
{
    /// <summary>
    /// A binding used to call the FHIR server and couples input/output pipelines or acts as terminator for an input pipeline.
    /// </summary>
    public class FhirBinding : IBinding
    {

        /// <summary>
        /// Creates an instanc of the FhirBinding.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="authenticator"></param>
        /// <param name="logger"></param>
        public FhirBinding(IOptions<FhirBindingOptions> options, IAuthenticator authenticator, ILogger<FhirBinding> logger = null)
        {
            this.options = options;
            this.authenticator = authenticator;
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly IOptions<FhirBindingOptions> options;
        private readonly IAuthenticator authenticator;
        private readonly ILogger logger;

        /// <summary>
        /// Gets the name of the binding "FhirBinding".
        /// </summary>
        public string Name => "FhirBinding";

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
            logger?.LogInformation("{0}-{1} fhir binding received.", Name, Id);

            if (context == null)
            {
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            try
            {
                string userAssertion = authenticator.RequiresOnBehalfOf ? context.Request.Headers.Authorization.Parameter.TrimStart("Bearer ".ToCharArray()) : null;

                string securityToken = await authenticator.AquireTokenForClientAsync(options.Value.FhirServerUrl, null, null, null, userAssertion, CancellationToken.None);
                RestRequestBuilder builder = new(context.Request.Method.ToString(),
                                                                    context.Request.RequestUri.AbsoluteUri.Replace(context.Request.RequestUri.PathAndQuery, ""),
                                                                    securityToken,
                                                                    context.Request.RequestUri.LocalPath,
                                                                    context.Request.RequestUri.Query,
                                                                    context.Request.GetHeaders(),
                                                                    context.Request.Content == null ? null : await context.Request.Content.ReadAsByteArrayAsync(),
                                                                    "application/json");
                RestRequest req = new(builder);
                var resp = await req.SendAsync();

                resp.EnsureSuccessStatusCode();
                context.StatusCode = resp.StatusCode;
                context.Content = await resp.Content?.ReadAsByteArrayAsync();
                OnComplete?.Invoke(this, new BindingCompleteEventArgs(Id, Name, context));
                logger?.LogInformation("{0}-{1} completed.", Name, Id);
                return context;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{0}-{1} fault with FHIR server request.", Name, Id);
                context.IsFatal = true;
                context.Error = ex;
                context.Content = null;
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, ex));
                logger?.LogInformation("{0}-{1} signaled error.", Name, Id);
                return null;
            }
        }

    }
}
