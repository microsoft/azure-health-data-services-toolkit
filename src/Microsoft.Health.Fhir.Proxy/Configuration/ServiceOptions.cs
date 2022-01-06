using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    public class ServiceOptions
    {
        private X509Certificate2 certificate;

        public string ClientId { get; set; }


        public string ClientSecret { get; set; }


        public string KeyVaultUri { get; set; }

        public string KeyVaultCertificateName { get; set; }

        public bool SystemManagedIdentity
        {
            get { return string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(KeyVaultCertificateName); }
        }

        public string TenantId { get; set; }


        public string FhirServerUrl { get; set; }

        public string InstrumentationKey { get; set; }

        public LogLevel LoggingLevel { get; set; }

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
    }
}
