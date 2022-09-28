using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Security
{
    /// <summary>
    /// Interface to be implemented by class that acquires access tokens from Azure Active Directory.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Gets an indicator that determines with token acquisition of OnBehalfOf, i.e., constrained delegation.
        /// </summary>
        public bool RequiresOnBehalfOf { get; }




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
        Task<string> AcquireTokenForClientAsync(string resource,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? userAssertion = null,
                                                            CancellationToken cancellationToken = default);

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
        public Task<string> AcquireTokenForClientAsync(string resource,
                                                      TokenCredential credential,
                                                      string[] scopes = null,
                                                      string? parentRequestId = null,
                                                      string? claims = null,
                                                      string? tenantId = null,
                                                      CancellationToken cancellationToken = default);
    }
}
