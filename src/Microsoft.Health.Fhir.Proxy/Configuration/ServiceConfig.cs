using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    [JsonObject]
    [Serializable]
    public class ServiceConfig
    {
        public ServiceConfig()
        {
        }

        private X509Certificate2 certificate;

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("keyVaultUri")]
        public string KeyVaultUri { get; set; }

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

        [JsonIgnore]
        public bool SystemManagedIdentity
        {
            get { return string.IsNullOrEmpty(ClientId); }
        }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("fhirServiceUrl")]
        public string FhirServerUrl { get; set; }

        [JsonProperty("instrumentationKey")]
        public string InstrumentationKey { get; set; }

        [JsonProperty("logLevel")]
        public LogLevel LoggingLevel { get; set; }

    }
}
