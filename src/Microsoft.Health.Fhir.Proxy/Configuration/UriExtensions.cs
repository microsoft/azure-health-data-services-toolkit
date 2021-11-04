using System;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    public static class UriExtensions
    {
        public static Uri RemoveRoutePrefix(this Uri uri, string routePrefix)
        {
            if (string.IsNullOrEmpty(routePrefix))
            {
                return new Uri(uri.ToString());
            }

            string routePrefix2 = "/" + routePrefix.Trim('/');
            Uri uri2 = new(uri.ToString());
            string path = uri2.LocalPath.Replace(routePrefix2, "");
            UriBuilder builder = new()
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
                Path = path,
                Query = uri.Query
            };
            return new Uri(builder.ToString());
        }
    }
}
