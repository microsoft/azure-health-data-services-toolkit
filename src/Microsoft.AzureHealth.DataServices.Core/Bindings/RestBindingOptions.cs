using System;
using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// Options for REST binding.
    /// </summary>
    public class RestBindingOptions : ClientOptions
    {

        /// <summary>
        /// Gets or sets the server URL to call.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the scopes required to call the server.  This is purely optional and used with non-default scopes are required.
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Adds the response headers from the binding to the pipeline
        /// </summary>
        public bool AddResponseHeaders { get; set; } = true;

        /// <summary>
        /// Azure credential to be used by the binding.
        /// </summary>
        public TokenCredential? Credential { get; set; }
    }
}
