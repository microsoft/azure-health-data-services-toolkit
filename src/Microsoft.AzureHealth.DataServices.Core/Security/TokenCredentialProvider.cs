using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Health.Client.Authentication;

namespace Microsoft.AzureHealth.DataServices.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class TokenCredentialProvider : CredentialProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="scopes"></param>
        /// <param name="tenantId"></param>
        public TokenCredentialProvider(TokenCredential credential, string[]? scopes = null, string? tenantId = null)
        {
            _credential = credential;
            _scopes = scopes ?? Array.Empty<string>();
            _tenantId = tenantId;
        }

        private readonly TokenCredential _credential;
        private readonly string[]? _scopes;
        private readonly string? _tenantId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<string> BearerTokenFunction(CancellationToken cancellationToken = default)
        {
            var context = new TokenRequestContext(scopes: _scopes, tenantId: _tenantId);
            var token = await _credential.GetTokenAsync(context, cancellationToken);
            return token.Token;
        }
    }
}
