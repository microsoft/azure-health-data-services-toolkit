using System;
using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// Common options for bindings that use HttpClient.
    /// </summary>
    public interface IBindingOptions
    {
        /// <summary>
        /// Gets or sets the BaseAddress for the HttpClient used by the binding.
        /// </summary>
        Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the Credential for the HttpClient used by the binding.
        /// </summary>
        TokenCredential Credential { get; set; }

        /// <summary>
        /// Gets or sets the scopes required to call the server.  This is purely optional and used with non-default scopes are required.
        /// </summary>
        string[] Scopes { get; set; }
    }
}
