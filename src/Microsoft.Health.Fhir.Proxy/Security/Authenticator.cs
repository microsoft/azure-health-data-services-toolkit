using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Security
{

    /// <summary>
    /// Authenticator class for Azure Active Directory.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        public Authenticator(IOptions<ServiceIdentityOptions> options)
        {
            this.options = options;
        }

        private readonly IOptions<ServiceIdentityOptions> options;

        public bool RequiresOnBehalfOf
        {
            get { return (options.Value.CredentialType == ClientCredentialType.OnBehalfOfUsingCertificate || options.Value.CredentialType == ClientCredentialType.OnBehalfOfUsingClientSecert); }
        }

        /// <summary>
        /// Gets an access token via OAuth from Azure Active Directory.
        /// </summary>
        /// <param name="resource">The resource to access.</param>
        /// <param name="credential">Represents a credential capable of providing an OAuth token.</param>
        /// <param name="scopes">he scopes required for the token.</param>
        /// <param name="parentRequestId">The ClientRequestId of the request requiring a token for authentication, if applicable.</param>
        /// <param name="claims">Additional claims to be included in the token. See https://openid.net/specs/openid-connect-core-1_0-final.html#ClaimsParameter for more information on format and content.</param>
        /// <param name="tenantId">The tenantId to be included in the token request. If tenantId supplied in ServiceConfig, this will be the default if argument is null.</param>
        /// <param name="cancellationToken">The CancellationToken to use.</param>
        /// <returns></returns>
        public async Task<string> AquireTokenForClientAsync(string resource,
                                                            TokenCredential credential,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? tenantId = null,
                                                            CancellationToken cancellationToken = default)
        {
            scopes ??= GetDefaultScopes(resource);
            tenantId ??= options.Value.TenantId;
            TokenRequestContext context = new(scopes, parentRequestId, claims, tenantId);
            var tokenResult = await credential.GetTokenAsync(context, cancellationToken);
            return tokenResult.Token;
        }
        public async Task<string> AquireTokenForClientAsync(string resource,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? userAssertion = null,
                                                            CancellationToken cancellationToken = default)
        {
            var token = options.Value.CredentialType switch
            {
                ClientCredentialType.ManagedIdentity => await AquireTokenForClientAsync(resource,
                                                                                        new ManagedIdentityCredential(options.Value.ClientId),
                                                                                        scopes,
                                                                                        parentRequestId,
                                                                                        claims,
                                                                                        options.Value.TenantId,
                                                                                        cancellationToken),
                ClientCredentialType.ClientSecret => await AquireTokenForClientAsync(resource,
                                                                                     new ClientSecretCredential(options.Value.TenantId,
                                                                                                                options.Value.ClientId,
                                                                                                                options.Value.ClientSecret),
                                                                                     scopes, parentRequestId, claims, 
                                                                                     options.Value.TenantId,
                                                                                     cancellationToken),
                ClientCredentialType.Certificate => await AquireTokenForClientAsync(resource,
                                                                                     new ClientCertificateCredential(options.Value.TenantId,
                                                                                                                    options.Value.ClientId,
                                                                                                                    options.Value.Certficate),
                                                                                     scopes, parentRequestId, claims,
                                                                                     options.Value.TenantId,
                                                                                     cancellationToken),
                ClientCredentialType.OnBehalfOfUsingClientSecert => await AquireTokenForClientAsync(resource,
                                                                                   new OnBehalfOfCredential(options.Value.TenantId,
                                                                                                            options.Value.ClientId,
                                                                                                            options.Value.ClientSecret,
                                                                                                            userAssertion),
                                                                                   scopes, parentRequestId, claims,
                                                                                   options.Value.TenantId,
                                                                                   cancellationToken),
                ClientCredentialType.OnBehalfOfUsingCertificate => await AquireTokenForClientAsync(resource,
                                                                                   new OnBehalfOfCredential(options.Value.TenantId,
                                                                                                            options.Value.ClientId,
                                                                                                            options.Value.Certficate,
                                                                                                            userAssertion),
                                                                                   scopes, parentRequestId, claims,
                                                                                   options.Value.TenantId,
                                                                                   cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(options.Value.CredentialType), "Invalid parameters to acquire token from Azure AD."),
            };

            return token;
        }

        private string[] GetDefaultScopes(string resource)
        {
            return new string[] { $"{resource.TrimEnd('/')}/.default" };
        }
    }
}
