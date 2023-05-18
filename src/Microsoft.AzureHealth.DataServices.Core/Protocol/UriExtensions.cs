using System;

namespace Microsoft.AzureHealth.DataServices.Protocol
{
    /// <summary>
    /// Extensions for URIs.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Removes a route prefix in a URI.
        /// </summary>
        /// <param name="uri">URI to remove route prefix.</param>
        /// <param name="routePrefix">Route prefix to remove.</param>
        /// <returns>Uri without prefex</returns>
        public static Uri RemoveRoutePrefix(this Uri uri, string routePrefix)
        {
            if (string.IsNullOrEmpty(routePrefix))
            {
                return new Uri(uri.ToString());
            }

            var routePrefix2 = "/" + routePrefix.Trim('/');
            Uri uri2 = new(uri.ToString());
            var path = uri2.LocalPath.Replace(routePrefix2, string.Empty);
            UriBuilder builder = new()
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
                Path = path,
                Query = uri.Query,
            };
            return new Uri(builder.ToString());
        }
    }
}
