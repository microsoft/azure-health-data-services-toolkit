using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    [Serializable]
    [JsonObject]
    public class CertificateConfig
    {
        private X509Certificate2 certificate;

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("keyVaultUri")]
        public string KeyVaultUri { get; set; }

        [JsonProperty("keyVaultCertificateName")]
        public string KeyVaultCertificateName { get; set; }

        public X509Certificate2 Certficate
        {
            get
            {
                if(certificate == null)
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
