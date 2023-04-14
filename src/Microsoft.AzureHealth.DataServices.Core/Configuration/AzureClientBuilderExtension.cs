using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Extensions;
using Azure.Data.AppConfiguration;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Clients;

namespace Microsoft.AzureHealth.DataServices.Configuration
{
    /// <summary>
    /// Helper extension for Azure Client Builder.
    /// </summary>
    public static class AzureClientBuilderExtension
    {
        /// <summary>
        /// Adds Generic Rest Client.
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="uri">Fhir server url.</param>
        /// <returns></returns>
        public static IAzureClientBuilder<GenericRestClient, RestBindingOptions> AddGenericRestClient<TBuilder>(this TBuilder builder, Uri uri)
           where TBuilder : IAzureClientFactoryBuilderWithCredential
        {
            return builder.RegisterClientFactory<GenericRestClient, RestBindingOptions>((options, cred) => new GenericRestClient(uri, cred, options));
        }

    }
}
