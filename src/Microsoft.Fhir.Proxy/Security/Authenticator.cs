using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Security
{
    public class Authenticator
    {
        public Authenticator(string resource, string clientId, string clientSecret, string tenantId)
        {
            Resource = resource;
            ClientId = clientId;
            ClientSecret = clientSecret;
            TenantId = tenantId;
        }

        public Authenticator(string resource, X509Certificate2 certificate, string tenantId)
        {
            Resource = resource;
            Certificate = certificate;
            TenantId = tenantId;
        }
        public string ClientId { get; private set; }

        public string ClientSecret { get; private set; }

        public string TenantId { get; private set; }

        public string Resource { get; private set; }

        public X509Certificate2 Certificate { get; private set; }

        public async Task<string> AcquireTokenForClientAsync(string[] scopes = null)
        {
            var app = GetApp();

            scopes ??= GetDefaultScopes();

            var result = await app.AcquireTokenForClient(scopes)
                       .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                       .ExecuteAsync();

            return result.AccessToken;
        }

        public async Task<string> AcquireTokenOnBehalfOfAsync(string bearerToken, string[] scopes = null)
        {
            var app = GetApp();
            scopes ??= GetDefaultScopes();
            var result = await app.AcquireTokenOnBehalfOf(scopes, new UserAssertion(bearerToken))
                .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                .ExecuteAsync();

            return result.AccessToken;
        }

        private string[] GetDefaultScopes()
        {
            return new string[] { $"{Resource}/.default" };
        }

        private IConfidentialClientApplication GetApp()
        {
            if (Certificate == null)
            {
                return ConfidentialClientApplicationBuilder.Create(ClientId)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                        .Build();
            }
            else
            {
                return ConfidentialClientApplicationBuilder.Create(ClientId)
                   .WithCertificate(Certificate)
                   .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                   .Build();
            }
        }
    }
}
