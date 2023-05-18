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
    }
}
