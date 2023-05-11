using System;
using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// Options for REST binding.
    /// </summary>
    public class RestBindingOptions : IBindingOptions
    {
        /// <summary>
        /// Gets or sets the server URL to call.
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the credential used to call the server.
        /// </summary>
        public TokenCredential? Credential { get; set; } = null;

        /// <summary>
        /// Gets or sets the setting controlling if the caller's access token is passed through to the server.
        /// </summary>
        public bool PassThroughAuthorizationHeader { get; set; } = false;

        /// <summary>
        /// Gets or sets the scopes required to call the server.  This is purely optional and used with non-default scopes are required.
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Adds the response headers from the binding to the pipeline
        /// </summary>
        public bool AddResponseHeaders { get; set; } = true;
        
    }
}
