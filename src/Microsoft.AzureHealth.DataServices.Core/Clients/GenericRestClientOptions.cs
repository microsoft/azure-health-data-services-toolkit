using Azure.Core;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Inherited options class to allow users to configure requests sent to GenericRestClient
    /// </summary>
    public class GenericRestClientOptions : ClientOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRestClientOptions"/> class.
        /// </summary>
        public GenericRestClientOptions()
        { 
        }
    }
}