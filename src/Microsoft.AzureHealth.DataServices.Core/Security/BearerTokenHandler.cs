﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 
        /// </summary>
        /// <param name="tokenCredential"></param>
        /// <param name="scopes"></param>
        public BearerTokenHandler(TokenCredential tokenCredential, string[]? scopes)
            : this(tokenCredential, scopes, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30))
        {
        }

        internal BearerTokenHandler(
            TokenCredential tokenCredential,
            string[]? scopes,
            TimeSpan tokenRefreshOffset,
            TimeSpan tokenRefreshRetryDelay)
        {
            EnsureArg.IsNotNull(tokenCredential, nameof(tokenCredential));
            _scopes = scopes;
            _accessTokenCache = new AccessTokenCache(tokenCredential, _scopes, tokenRefreshOffset, tokenRefreshRetryDelay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException("Bearer token authentication is not permitted for non TLS protected (https) endpoints.");
            }

            try
            {
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
            catch (AuthenticationFailedException ex)
            {
                throw ex;
            }
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
                            string[] scopes,
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
