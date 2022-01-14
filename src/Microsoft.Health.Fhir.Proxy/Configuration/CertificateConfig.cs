using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    /// <summary>
    /// Configuration of certificates to be obtained from a Key Vault.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class CertificateConfig
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// Gets for sets the AAD tenant id.
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets for sets the client id used to authenticate.
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret used to authenticate.
        /// </summary>
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the Key Vault URI used to hold the certificate.
        /// </summary>
        [JsonProperty("keyVaultUri")]
        public string KeyVaultUri { get; set; }

        /// <summary>
        /// Gets or sets the certificate name in key vault.
        /// </summary>
        [JsonProperty("keyVaultCertificateName")]
        public string KeyVaultCertificateName { get; set; }

        /// <summary>
        /// Gets the certificate from the key vault.
        /// </summary>
        public X509Certificate2 Certficate
        {
            get
            {
                if (certificate == null)
                {
                    ClientSecretCredential cred = new(TenantId, ClientId, ClientSecret);
                    CertificateClient client = new(new Uri(KeyVaultUri), cred);
                    Response<KeyVaultCertificateWithPolicy> resp = client.GetCertificate(KeyVaultCertificateName);
                    certificate = new(resp.Value.Cer);
                }

                return certificate;
            }
        }
    }
}
