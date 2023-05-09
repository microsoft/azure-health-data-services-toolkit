using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Client;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// A binding used to call the FHIR Service and couples input/output pipelines or acts as terminator for an input pipeline.
    /// </summary>
    public class FhirBinding : IBinding
    {

        /// <summary>
        /// Creates an instance of <see cref="FhirBinding"/>.
        /// </summary>
        /// <param name="options">Options class to configure the Fhir Binding.</param>
        /// <param name="fhirClient">Fhir client used for operations against the FHIR Server.</param>
        /// <param name="logger">Optional logger.</param>
        public FhirBinding(IOptions<FhirBindingOptions> options, FhirClient fhirClient, ILogger<FhirBinding> logger = null)
        {
            _options = options;
            _fhirClient = fhirClient;
            _logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly IOptions<FhirBindingOptions> _options;
        private readonly FhirClient _fhirClient;
        private readonly ILogger _logger;


        /// <summary>
        /// Gets the name of the binding "FhirBinding".
        /// </summary>
        public string Name => nameof(FhirBinding);

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
            _logger?.LogInformation("{Name}-{Id} binding received.", Name, Id);

            if (context == null)
            {
                OnError?.Invoke(this, new BindingErrorEventArgs(Id, Name, new ArgumentNullException(nameof(context))));
                return null;
            }

            try
            {
                NameValueCollection headers = context.Headers.RequestAppendAndReplace(context.Request, false);

                context.Request.


                var resp = await req.SendAsync();
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

        private Func<CancellationToken, Task<FhirResponse>> BuildFhirRequest(HttpRequestMessage request, CancellationToken cancel = default)
        {
            var path = new FhirUriPath(request.Method, request.RequestUri);

            if (path.Operation is not null)
            {
                // Handle operation check
                if (path.Id is not null)
                {
                    if (string.Equals(path.Method, "GET", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return async (cancel) =>
                        {
                            return path.Operation switch
                            {
                                FhirOperationType.Reindex => await _fhirClient.CheckReindexAsync(new Uri(path.NormalizedPath), cancel),
                                FhirOperationType.Import => new FhirResponse(await _fhirClient.CheckImportAsync(new Uri(path.NormalizedPath), cancel)),
                                FhirOperationType.Export => new FhirResponse(await _fhirClient.CheckExportAsync(new Uri(path.NormalizedPath), cancel)),
                                _ => throw new NotImplementedException(),
                            };
                        };
                    }
                    // Handle operation delete
                    else if (string.Equals(path.Method, "DELETE", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return async (cancel) =>
                        {
                            switch (path.Operation)
                            {
                                case FhirOperationType.Import:
                                    return new FhirResponse(await _fhirClient.CancelImport(new Uri(path.NormalizedPath), cancel));
                                case FhirOperationType.Export:
                                    await _fhirClient.CancelExport(new Uri(path.NormalizedPath), cancel);
                                    return new FhirResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                                default:
                                    throw new NotImplementedException();
                            }
                        };
                    }
                }
                // Handle new operation requests
                else
                {
                    return async (cancel) =>
                    {
                        return path.Operation switch
                        {
                            FhirOperationType.Reindex => await _fhirClient.CheckReindexAsync(new Uri(path.NormalizedPath), cancel),
                            FhirOperationType.Import => new FhirResponse(await _fhirClient.CheckImportAsync(new Uri(path.NormalizedPath), cancel)),
                            FhirOperationType.Export => new FhirResponse(await _fhirClient.CheckExportAsync(new Uri(path.NormalizedPath), cancel)),
                            _ => throw new NotImplementedException(),
                        };
                    }
            }

            throw new NotImplementedException();
        }
    }
}
