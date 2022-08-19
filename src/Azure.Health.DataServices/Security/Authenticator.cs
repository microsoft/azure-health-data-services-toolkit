using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace Azure.Health.DataServices.Security
{

    /// <summary>
    /// Authenticator class for acquiring access tokens from Azure Active Directory.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        /// <summary>
        /// Creates an instance of Authenticator.
        /// </summary>
        /// <param name="options">ServiceIdentity options used by the authenticator.</param>
        public Authenticator(IOptions<ServiceIdentityOptions> options)
        {
            this.options = options;
        }

        private readonly IOptions<ServiceIdentityOptions> options;

        /// <summary>
        /// Gets a indication whether OBO is required for access token acquisition.
        /// </summary>
        public bool RequiresOnBehalfOf
        {
            get { return (options.Value.CredentialType == ClientCredentialType.OnBehalfOfUsingCertificate || options.Value.CredentialType == ClientCredentialType.OnBehalfOfUsingClientSecert); }
        }

        /// <summary>
        /// Gets an access token via OAuth from Azure Active Directory.
        /// </summary>
        /// <param name="resource">The resource to access.</param>
        /// <param name="credential">Represents a credential capable of providing an OAuth token.</param>
        /// <param name="scopes">Scopes required for the token.</param>
        /// <param name="parentRequestId">The ClientRequestId of the request requiring a token for authentication, if applicable.</param>
        /// <param name="claims">Additional claims to be included in the token. See https://openid.net/specs/openid-connect-core-1_0-final.html#ClaimsParameter for more information on format and content.</param>
        /// <param name="tenantId">The tenantId to be included in the token request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Access token.</returns>
        public async Task<string> AcquireTokenForClientAsync(string resource,
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

        /// <summary>
        /// Gets an access token via OAuth from Azure Active Directory.
        /// </summary>
        /// <param name="resource">Resource requesting access.</param>
        /// <param name="scopes">Scopes required for the token.</param>
        /// <param name="parentRequestId">The ClientRequestId of the request requiring a token for authentication, if applicable.</param>
        /// <param name="claims">Additional claims to be included in the token. See https://openid.net/specs/openid-connect-core-1_0-final.html#ClaimsParameter for more information on format and content.</param>
        /// <param name="userAssertion">Access token required when using OnBehalfOf.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Access token.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<string> AcquireTokenForClientAsync(string resource,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? userAssertion = null,
                                                            CancellationToken cancellationToken = default)
        {
            var token = options.Value.CredentialType switch
            {
                ClientCredentialType.ManagedIdentity => await AcquireTokenForClientAsync(resource,
                                                                                        new ManagedIdentityCredential(options.Value.ClientId),
                                                                                        scopes,
                                                                                        parentRequestId,
                                                                                        claims,
                                                                                        options.Value.TenantId,
                                                                                        cancellationToken),
                ClientCredentialType.ClientSecret => await AcquireTokenForClientAsync(resource,
                                                                                     new ClientSecretCredential(options.Value.TenantId,
                                                                                                                options.Value.ClientId,
                                                                                                                options.Value.ClientSecret),
                                                                                     scopes, parentRequestId, claims,
                                                                                     options.Value.TenantId,
                                                                                     cancellationToken),
                ClientCredentialType.Certificate => await AcquireTokenForClientAsync(resource,
                                                                                     new ClientCertificateCredential(options.Value.TenantId,
                                                                                                                    options.Value.ClientId,
                                                                                                                    options.Value.Certficate),
                                                                                     scopes, parentRequestId, claims,
                                                                                     options.Value.TenantId,
                                                                                     cancellationToken),
                ClientCredentialType.OnBehalfOfUsingClientSecert => await AcquireTokenForClientAsync(resource,
                                                                                   new OnBehalfOfCredential(options.Value.TenantId,
                                                                                                            options.Value.ClientId,
                                                                                                            options.Value.ClientSecret,
                                                                                                            userAssertion),
                                                                                   scopes, parentRequestId, claims,
                                                                                   options.Value.TenantId,
                                                                                   cancellationToken),
                ClientCredentialType.OnBehalfOfUsingCertificate => await AcquireTokenForClientAsync(resource,
                                                                                   new OnBehalfOfCredential(options.Value.TenantId,
                                                                                                            options.Value.ClientId,
                                                                                                            options.Value.Certficate,
                                                                                                            userAssertion),
                                                                                   scopes, parentRequestId, claims,
                                                                                   options.Value.TenantId,
                                                                                   cancellationToken),
                _ => await AcquireTokenForClientAsync(resource,
                                                    new DefaultAzureCredential(),
                                                    scopes, parentRequestId, claims,
                                                    options.Value.TenantId, cancellationToken),

            };

            return token;
        }

        private static string[] GetDefaultScopes(string resource)
        {
            return new string[] { $"{resource.TrimEnd('/')}/.default" };
        }
    }
}
