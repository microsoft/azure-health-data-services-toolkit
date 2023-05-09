using System;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Clients;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    /// <summary>
    /// Configuration for <see cref="FhirClient" />
    /// </summary>
    [Serializable]
    [JsonObject]
    public class FhirClientConfig
    {
        /// <summary>
        /// Creates an instance of <see cref="FhirClientConfig" />.
        /// </summary>
        public FhirClientConfig()
        {
        }

        /// <summary>
        /// Creates in instance of <see cref="FhirClientConfig" />.
        /// </summary>
        /// <param name="fhirUrl">Url to the FHIR Service used for testing</param>
        public FhirClientConfig(string fhirServerUrl)
        {
            this.FhirServerUrl = fhirServerUrl;
        }

        /// <summary>
        /// Gets or sets Fhir Server URL used for testing.
        /// </summary>
        [JsonProperty("fhirServerUrl")]
        public string FhirServerUrl { get; set; }

        /// <summary>
        /// Gets or sets client id used for testing.
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets client secret used for testing.
        /// </summary>
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets tenant id used for testing.
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
    }
}
