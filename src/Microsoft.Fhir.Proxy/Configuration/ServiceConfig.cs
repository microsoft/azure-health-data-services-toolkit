using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Fhir.Proxy.Configuration
{
    [JsonObject]
    [Serializable]
    public class ServiceConfig
    {
        public ServiceConfig()
        {
            ClientId ??= Environment.GetEnvironmentVariable(Constants.ClientId) ?? null;
            ClientSecret ??= Environment.GetEnvironmentVariable(Constants.ClientSecret) ?? null;
            Tenant ??= Environment.GetEnvironmentVariable(Constants.TenantId) ?? null;
            FhirServerUrl ??= Environment.GetEnvironmentVariable(Constants.FhirServerUrl) ?? null;
            KeyVaultUriString ??= Environment.GetEnvironmentVariable(Constants.KeyVaultUriString) ?? null;
            KeyVaultCertificateName ??= Environment.GetEnvironmentVariable(Constants.KeyVaultCertificateName) ?? null;
            InstrumentationKey ??= Environment.GetEnvironmentVariable(Constants.InstrumentationKey) ?? null;
            string logLevel = Environment.GetEnvironmentVariable(Constants.LogLevel) ?? null;
            _ = Enum.TryParse(logLevel, out LogLevel loggingLevel);
            LoggingLevel = loggingLevel;
        }

        private X509Certificate2 certificate;

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("keyVaultUriString")]
        public string KeyVaultUriString { get; set; }

        [JsonProperty("keyVaultCertificateName")]
        public string KeyVaultCertificateName { get; set; }

        [JsonIgnore]
        public X509Certificate2 Certficate
        {
            get
            {
                if (!string.IsNullOrEmpty(KeyVaultCertificateName)
                        && !string.IsNullOrEmpty(KeyVaultUriString)
                        && certificate == null)
                {
                    ClientSecretCredential cred = new(Tenant, ClientId, ClientSecret);
                    CertificateClient client = new(new Uri(KeyVaultUriString), cred);
                    Response<KeyVaultCertificateWithPolicy> resp = client.GetCertificate(KeyVaultCertificateName);
                    certificate = new(resp.Value.Cer);
                }

                return certificate;
            }
        }

        [JsonProperty("tenant")]
        public string Tenant { get; set; }

        [JsonProperty("fhirServiceUrl")]
        public string FhirServerUrl { get; set; }

        [JsonProperty("instrumentationKey")]
        public string InstrumentationKey { get; set; }

        [JsonProperty("logLevel")]
        public LogLevel LoggingLevel { get; set; }

    }
}
