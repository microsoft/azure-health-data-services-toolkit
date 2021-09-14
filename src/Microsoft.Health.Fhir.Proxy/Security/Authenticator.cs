using Microsoft.Identity.Client;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Proxy.Configuration;
using Azure.Core;
using System.Security;

namespace Microsoft.Health.Fhir.Proxy.Security
{
    public class Authenticator
    {
        public Authenticator(string resource, ServiceConfig config)
        {
            this.resource = resource;
            this.config = config;
        }

        private readonly ServiceConfig config;
        private readonly string resource;

        public string ClientId => config.ClientId;

        public string ClientSecret => config.ClientSecret;

        public string TenantId => config.TenantId;

        public string Resource => resource;

        public X509Certificate2 Certificate => config.Certficate;

        public async Task<string> AcquireTokenForClientAsync(string[] scopes = null)
        {
            scopes ??= GetDefaultScopes();

            if (config.SystemManagedIdentity)
            {
                DefaultAzureCredential credential = string.IsNullOrEmpty(ClientId) ? new(false) : new(new DefaultAzureCredentialOptions() { ManagedIdentityClientId = ClientId });
                TokenRequestContext context = new(scopes);
                var tokenResult = await credential.GetTokenAsync(context);
                return tokenResult.Token;
            }
            else
            {
                var app = GetApp();
                var result = await app.AcquireTokenForClient(scopes)
                           .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                           .ExecuteAsync();
                return result.AccessToken;
            }
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
            return new string[] { $"{Resource.TrimEnd('/')}/.default" };
        }

        private IConfidentialClientApplication GetApp()
        {
            if (ClientSecret != null)
            {
                return ConfidentialClientApplicationBuilder.Create(ClientId)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                        .Build();
            }
            else if (Certificate != null)
            {
                return ConfidentialClientApplicationBuilder.Create(ClientId)
                   .WithCertificate(Certificate)
                   .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                   .Build();
            }
            else
            {
                throw new SecurityException("No secret found to authenticate.");
            }
        }
    }
}
