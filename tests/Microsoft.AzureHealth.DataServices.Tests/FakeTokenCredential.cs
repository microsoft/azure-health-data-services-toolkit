using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AzureHealth.DataServices.Tests
{
    internal class FakeTokenCredential : TokenCredential
    {
        public Func<TokenRequestContext, CancellationToken, AccessToken> TokenFactory { get; set; }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (TokenFactory != null)
            {
                return TokenFactory(requestContext, cancellationToken);
            }
            else
            {
                return new AccessToken("mockToken", DateTimeOffset.UtcNow.AddMinutes(10));
            }
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
        }
    }
}
