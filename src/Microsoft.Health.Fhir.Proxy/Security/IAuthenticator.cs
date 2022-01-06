using Azure.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Security
{
    public interface IAuthenticator
    {
        public bool RequiresOnBehalfOf { get; }

        Task<string> AquireTokenForClientAsync(string resource,
                                                            string[] scopes = null,
                                                            string? parentRequestId = null,
                                                            string? claims = null,
                                                            string? userAssertion = null,
                                                            CancellationToken cancellationToken = default);

        public Task<string> AquireTokenForClientAsync(string resource,
                                                      TokenCredential credential,
                                                      string[] scopes = null,
                                                      string? parentRequestId = null,
                                                      string? claims = null,
                                                      string? tenantId = null,
                                                      CancellationToken cancellationToken = default);
}
}
