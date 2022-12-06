using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Caching;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        /// <param name="jsonObjectCache">Rest binding options.</param>
        /// <param name="authenticator">Optional authenticator to acquire security token.</param>
        /// <param name="logger">Optional logger.</param>
        public RestBinding(IOptions<RestBindingOptions> options, IAuthenticator authenticator = null, IJsonObjectCache jsonObjectCache = null, ILogger<RestBinding> logger = null)
        {
            this.options = options;
            this.authenticator = authenticator;
            this.jsonObjectCache = jsonObjectCache;
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
        }

        private readonly IOptions<RestBindingOptions> options;
        private readonly IAuthenticator authenticator;
        private readonly IJsonObjectCache jsonObjectCache;
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

        private string TokenName => "SecurityToken";

        private int MaxAttempt => 3;

        private TimeSpan RetryTime => TimeSpan.FromSeconds(1.0);

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
                string securityToken = null;
                if (authenticator != null)
                {
                    if (jsonObjectCache != null)
                    {
                        securityToken = await jsonObjectCache.GetAsync(TokenName);
                        if (string.IsNullOrEmpty(securityToken))
                        {
                            securityToken = await FetchToken(context);
                            await jsonObjectCache.AddAsync(TokenName, securityToken);
                        }
                        else
                        {
                            securityToken = JsonConvert.DeserializeObject<string>(securityToken);
                        }
                    }
                    else
                    {
                        securityToken = await FetchToken(context);
                    }
                }
                var resp = await GetRestRequest(context, securityToken).Result.SendAsync();
                if (resp.StatusCode == HttpStatusCode.Unauthorized && jsonObjectCache != null)
                {
                    await jsonObjectCache.RemoveAsync(TokenName);
                    securityToken = await FetchToken(context);
                    await jsonObjectCache.AddAsync(TokenName, securityToken);
                    RestRequest req = await GetRestRequest(context, securityToken);
                    resp = await Retry.ExecuteRequest(req, RetryTime, MaxAttempt);
                }
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

        /// <summary>
        /// Set the Token in In-Memory cache.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> FetchToken(OperationContext context)
        {
            string securityToken = null;
            if (string.IsNullOrEmpty(securityToken) && authenticator != null)
            {
                string userAssertion = authenticator.RequiresOnBehalfOf ? context.Request.Headers.Authorization.Parameter.TrimStart("Bearer ".ToCharArray()) : null;
                securityToken = await authenticator.AcquireTokenForClientAsync(options.Value.ServerUrl, options.Value.Scopes, null, null, userAssertion);
            }
            return securityToken;
        }

        /// <summary>
        /// Create Rest Request Object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="securityToken"></param>
        /// <returns></returns>
        private async Task<RestRequest> GetRestRequest(OperationContext context, string securityToken)
        {
            NameValueCollection headers = context.Headers.RequestAppendAndReplace(context.Request, false);
            RestRequestBuilder builder = new(context.Request.Method.ToString(),
                                                                    options.Value.ServerUrl,
                                                                    securityToken,
                                                                    context.Request.RequestUri.LocalPath,
                                                                    context.Request.RequestUri.Query,
                                                                    headers,
                                                                    context.Request.Content == null ? null : await context.Request.Content.ReadAsByteArrayAsync(),
                                                                    "application/json");
            RestRequest req = new(builder);
            return req;
        }

    }
}
