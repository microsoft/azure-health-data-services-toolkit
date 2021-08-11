using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Clients;
using Microsoft.Fhir.Proxy.Pipelines;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Bindings
{
    /// <summary>
    /// A binding used to call the FHIR server and couples input/output pipelines or acts as terminator for an input pipeline.
    /// </summary>
    public class FhirPipelineBinding : PipelineBinding
    {
        /// <summary>
        /// Creates an instanc of the FhirPipelineBinding.
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public FhirPipelineBinding(ILogger logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates an instance of the FhirPipelineBinding.
        /// </summary>
        /// <param name="certificate">X509 certificate used a security token when calling the FHIR server.</param>
        /// <param name="logger">Optional logger.</param>
        public FhirPipelineBinding(X509Certificate2 certificate, ILogger logger = null)
            : this(logger)
        {
            this.certificate = certificate;
        }

        private readonly ILogger logger;
        private readonly X509Certificate2 certificate;

        public override string Name => "FhirPipelineBinding";

        public override string Id { get; internal set; }

        public override event EventHandler<PipelineErrorEventArgs> OnError;

        public override event EventHandler<PipelineCompleteEventArgs> OnComplete;

        public override async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            logger?.LogInformation($"{Name}-{Id} received.");

            if (context == null)
            {
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            RestRequestBuilder builder = null;

            try
            {
                if (certificate != null)
                {
                    builder = new RestRequestBuilder(context.Request.Method.ToString(),
                                                                    context.Request.RequestUri.AbsoluteUri.Replace(context.Request.RequestUri.PathAndQuery, ""),
                                                                    certificate,
                                                                    context.Request.RequestUri.LocalPath,
                                                                    context.Request.RequestUri.Query,
                                                                    context.Request.GetHeaders(),
                                                                    await context.Request.Content.ReadAsByteArrayAsync(),
                                                                    Constants.ContentTypes.Json);
                }
                else
                {
                    string securityToken = context.Request.Headers.Contains("Authorization") ? context.Request.Headers.Authorization.Parameter.TrimStart("Bearer ".ToCharArray()) : null;
                    builder = new RestRequestBuilder(context.Request.Method.ToString(),
                                                                    context.Request.RequestUri.AbsoluteUri.Replace(context.Request.RequestUri.PathAndQuery, ""),
                                                                    securityToken,
                                                                    context.Request.RequestUri.LocalPath,
                                                                    context.Request.RequestUri.Query,
                                                                    context.Request.GetHeaders(),
                                                                    context.Request.Content == null ? null : await context.Request.Content.ReadAsByteArrayAsync(),
                                                                    "application/json");
                }

                RestRequest req = new(builder);
                var resp = await req.SendAsync();

                resp.EnsureSuccessStatusCode();
                context.StatusCode = resp.StatusCode;
                context.Content = await resp.Content?.ReadAsByteArrayAsync();
                OnComplete?.Invoke(this, new PipelineCompleteEventArgs(Id, Name, context));
                logger?.LogInformation($"{Name}-{Id} completed.");
                return context;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"{Name}-{Id} fault with FHIR server request.");
                context.IsFatal = true;
                context.Error = ex;
                context.Content = null;
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, ex));
                logger?.LogInformation($"{Name}-{Id} signaled error.");
                return null;
            }
        }
    }
}
