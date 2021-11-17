using Azure.Core;
using Azure.Identity;
using Microsoft.Health.Fhir.Proxy.Configuration;
using Microsoft.Identity.Client;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Security
{

    /// <summary>
    /// Authenticator class for Azure Active Directory.
    /// </summary>
    public class Authenticator
    {
        /// <summary>
        /// Creates an instance of authenticator.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="config"></param>
        public Authenticator(string resource, ServiceConfig config)
        {
            this.resource = resource;
            this.config = config;
        }

        private readonly ServiceConfig config;
        private readonly string resource;

        /// <summary>
        /// Gets the client_id.
        /// </summary>
        public string ClientId => config.ClientId;

        /// <summary>
        /// Gets the client_secret.
        /// </summary>
        public string ClientSecret => config.ClientSecret;

        /// <summary>
        /// Gets the tenant_id
        /// </summary>
        public string TenantId => config.TenantId;

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public string Resource => resource;

        /// <summary>
        /// Gets X509 certificate.
        /// </summary>
        public X509Certificate2 Certificate => config.Certficate;

        /// <summary>
        /// Acquires access token for client.
        /// </summary>
        /// <param name="scopes">Optional scopes.  The default is resource/.default</param>
        /// <returns></returns>
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
