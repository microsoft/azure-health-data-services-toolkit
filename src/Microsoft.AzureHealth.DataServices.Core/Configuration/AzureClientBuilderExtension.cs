using System;
using Azure.Core;
using Azure.Core.Extensions;
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
        public static IAzureClientBuilder<GenericRestClient, ClientOptions> AddGenericRestClient<TBuilder>(this TBuilder builder, Uri uri)
           where TBuilder : IAzureClientFactoryBuilderWithCredential
        {
            return builder.RegisterClientFactory<GenericRestClient, ClientOptions>((options, cred) => new GenericRestClient(uri, options, cred));
        }

    }
}
