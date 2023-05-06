using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Options for class for <see cref="FhirClient"/>.
    /// </summary>
    public class FhirClientOptions : ClientOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FhirClientOptions"/> class.
        /// </summary>
        public FhirClientOptions()
        {
        }

        /// <summary>
        /// Gets the custom <see cref="Scope"/> to be used when authenticating with the service.
        /// </summary>
        public string? Scope { get; set; }
    }
}
