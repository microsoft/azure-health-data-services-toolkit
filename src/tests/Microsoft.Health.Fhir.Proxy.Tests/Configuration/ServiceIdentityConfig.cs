using System;
using System.Security.Cryptography.X509Certificates;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Configuration
{
    public class ServiceIdentityConfig
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// AAD client id for authentication.
        /// </summary>
        /// /// <remarks>Property can be omitted with using either (i) MSI or (ii) X509 certificate for authentication</remarks>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// AAD client secret for authentication.
        /// </summary>
        /// /// <remarks>Property can be omitted with using either (i) MSI or (ii) X509 certificate for authentication</remarks>
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Key vault URI required obtaining a certificate for AAD authentication.
        /// </summary>
        /// <remarks>Property can be omitted with using either (i) MSI or (ii) client_id and client_secret for authentication</remarks>
        [JsonProperty("keyVaultUri")]
        public string KeyVaultUri { get; set; }

        /// <summary>
        /// Name of the certificate to retreive from key vault.
        /// </summary>
        /// <remarks>Property can be omitted with using either (i) MSI or (ii) client_id and client_secret for authentication</remarks>
        [JsonProperty("keyVaultCertificateName")]
        public string KeyVaultCertificateName { get; set; }

        [JsonIgnore]
        public X509Certificate2 Certficate
        {
            get
            {
                if (!string.IsNullOrEmpty(KeyVaultCertificateName)
                        && !string.IsNullOrEmpty(KeyVaultUri)
                        && certificate == null
                        && !string.IsNullOrEmpty(TenantId)
                        && !string.IsNullOrEmpty(ClientId)
                        && !string.IsNullOrEmpty(ClientSecret))
                {
                    ClientSecretCredential cred = new(TenantId, ClientId, ClientSecret);
                    CertificateClient client = new(new Uri(KeyVaultUri), cred);
                    Response<KeyVaultCertificateWithPolicy> resp = client.GetCertificate(KeyVaultCertificateName);
                    certificate = new(resp.Value.Cer);
                }

                return certificate;
            }
        }

        /// <summary>
        /// Gets an indicators that is true when MSI is used; otherwise false. 
        /// </summary>
        [JsonIgnore]
        public bool SystemManagedIdentity
        {
            get { return string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(KeyVaultCertificateName); }
        }

        /// <summary>
        /// Gets or sets Tenant ID used for AAD authentication.
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the FHIR server URL.
        /// </summary>
        [JsonProperty("fhirServiceUrl")]
        public string FhirServerUrl { get; set; }
    }
}
