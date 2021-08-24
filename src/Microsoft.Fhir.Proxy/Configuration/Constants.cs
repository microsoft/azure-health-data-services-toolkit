using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Configuration
{
    public class Constants
    {
        public const string TenantId = "PROXY_TENANT_ID";
        public const string ClientId = "PROXY_CLIENT_ID";
        public const string ClientSecret = "PROXY_CLIENT_SECRET";
        public const string FhirServerUrl = "PROXY_FHIR_SERVER_URL";
        public const string InstrumentationKey = "PROXY_INSTRUMENTATION_KEY";
        public const string LogLevel = "PROXY_LOG_LEVEL";
        public const string KeyVaultUriString = "PROXY_KEY_VAULT_URI";
        public const string KeyVaultCertificateName = "KEY_VAULT_CERTIFICATE_MAME";
    }
}
