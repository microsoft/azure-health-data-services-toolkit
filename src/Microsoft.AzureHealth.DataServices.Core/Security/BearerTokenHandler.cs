using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using EnsureThat;

namespace Microsoft.AzureHealth.DataServices.Security
{
    /// <summary>
    /// A HttpClient handler that sends an Access Token provided by a <see cref="TokenCredential"/> as an Authentication header.
    /// </summary>
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly string[]? _scopes;
        private readonly AccessTokenCache _accessTokenCache;

        /// <summary>
        /// Creates bearer token handler with default token cache settings.
        /// </summary>
        /// <param name="tokenCredential">Credential used to create tokens/</param>
        /// <param name="baseAddress">Base address for the client using the credential. Used for resource based scoping via {{baseAddress}}/.default</param>
        /// <param name="scopes">Optional scopes if you want to override the `.default` resource scope.</param>
        public BearerTokenHandler(TokenCredential tokenCredential, Uri baseAddress, string[]? scopes)
            : this(tokenCredential, baseAddress, scopes, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30))
        {
        }

        internal BearerTokenHandler(
            TokenCredential tokenCredential,
            Uri baseAddress,
            string[]? scopes,
            TimeSpan tokenRefreshOffset,
            TimeSpan tokenRefreshRetryDelay)
        {
            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));

            if (scopes is null or { Length: 0 })
            {
                EnsureArg.IsNotNull(baseAddress);
                _scopes = GetDefaultScopes(baseAddress);
            }
            else
            {
                _scopes = scopes;
            }

            _accessTokenCache = new AccessTokenCache(tokenCredential, tokenRefreshOffset, tokenRefreshRetryDelay);
        }

        /// <summary>
        /// Sends the request with the bearer token header.
        /// </summary>
        /// <param name="request">Incoming request message.</param>
        /// <param name="cancellationToken">Incoming cancellation token.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Only add header for requests that don't already have one.
            if (request.Headers.Authorization is not null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            if (request.RequestUri.Scheme != Uri.UriSchemeHttps && request.RequestUri.Host != "localhost")
            {
                throw new InvalidOperationException("Bearer token authentication is not permitted for non TLS protected (https) endpoints.");
            }

            var scopes = _scopes;
            if (scopes is null or { Length: 0 })
            {
                scopes = GetDefaultScopes(request.RequestUri);
            }
            AccessToken cachedToken = await _accessTokenCache.GetTokenAsync(scopes, cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken.Token);

            // Send the request.
            return await base.SendAsync(request, cancellationToken);
        }

        private static string[] GetDefaultScopes(Uri requestUri)
        {
            var baseAddress = requestUri.GetLeftPart(UriPartial.Authority);
            return new string[] { $"{baseAddress.TrimEnd('/')}/.default" };
        }

        private class AccessTokenCache
        {
            private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            private readonly TokenCredential _tokenCredential;
            private readonly TimeSpan _tokenRefreshOffset;
            private readonly TimeSpan _tokenRefreshRetryDelay;
            private AccessToken? _accessToken = null;
            private DateTimeOffset _accessTokenExpiration;

            public AccessTokenCache(
                           TokenCredential tokenCredential,
                            TimeSpan tokenRefreshOffset,
                            TimeSpan tokenRefreshRetryDelay)
            {
                _tokenCredential = tokenCredential;
                _tokenRefreshOffset = tokenRefreshOffset;
                _tokenRefreshRetryDelay = tokenRefreshRetryDelay;
            }
            public async Task<AccessToken> GetTokenAsync(string[] scopes, CancellationToken cancellationToken)
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (_accessToken is null || _accessTokenExpiration <= DateTimeOffset.UtcNow + _tokenRefreshOffset)
                    {
                        try
                        {
                            _accessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken).ConfigureAwait(false);
                            _accessTokenExpiration = _accessToken.Value.ExpiresOn;
                        }
                        catch (AuthenticationFailedException)
                        {
                            // If the token acquisition fails, retry after the delay.
                            await Task.Delay(_tokenRefreshRetryDelay, cancellationToken).ConfigureAwait(false);
                            _accessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken).ConfigureAwait(false);
                            _accessTokenExpiration = _accessToken.Value.ExpiresOn;
                        }
                    }
                    return _accessToken.Value;
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
        }
    }
}
