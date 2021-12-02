using Azure.Core;
using Microsoft.Health.Fhir.Proxy.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Security
{

    /// <summary>
    /// Authenticator class for Azure Active Directory.
    /// </summary>
    public class Authenticator
    {
        /// <summary>
        /// Creates an instance of authenticator.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="config"></param>
        public Authenticator(string resource, ServiceConfig config = null)
        {
            this.resource = resource;
            this.config = config;
        }

        private readonly ServiceConfig config;
        private readonly string resource;

        /// <summary>
        /// Gets the client_id.
        /// </summary>
        public string ClientId => config.ClientId;

        /// <summary>
        /// Gets the client_secret.
        /// </summary>
        public string ClientSecret => config.ClientSecret;

        /// <summary>
        /// Gets the tenant_id
        /// </summary>
        public string TenantId => config.TenantId;

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public string Resource => resource;

        /// <summary>
        /// Gets X509 certificate.
        /// </summary>
        public X509Certificate2 Certificate => config.Certficate;

        /// <summary>
        /// Gets an access token via OAuth from Azure Active Directory.
        /// </summary>
        /// <param name="credential">Represents a credential capable of providing an OAuth token.</param>
        /// <param name="scopes">he scopes required for the token.</param>
        /// <param name="parentRequestId">The ClientRequestId of the request requiring a token for authentication, if applicable.</param>
        /// <param name="claims">Additional claims to be included in the token. See https://openid.net/specs/openid-connect-core-1_0-final.html#ClaimsParameter for more information on format and content.</param>
        /// <param name="tenantId">The tenantId to be included in the token request. If tenantId supplied in ServiceConfig, this will be the default if argument is null.</param>
        /// <param name="cancellationToken">The CancellationToken to use.</param>
        /// <returns></returns>
        public async Task<string> AquireTokenForClientAsync(TokenCredential credential,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? tenantId = null,
                                                            CancellationToken cancellationToken = default)
        {
            scopes ??= GetDefaultScopes();
            tenantId ??= config?.TenantId;
            TokenRequestContext context = new(scopes, parentRequestId, claims, tenantId);
            var tokenResult = await credential.GetTokenAsync(context, cancellationToken);
            return tokenResult.Token;
        }
        private string[] GetDefaultScopes()
        {
            return new string[] { $"{Resource.TrimEnd('/')}/.default" };
        }
    }
}
