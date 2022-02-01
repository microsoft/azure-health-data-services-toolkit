using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Fhir.Proxy.Configuration;

namespace Fhir.Proxy.Protocol
{
    public class FhirPath : Uri
    {
        /// <summary>
        /// Creates an instance of FhirPath.
        /// </summary>
        /// <param name="method">HTTP method used in request.</param>
        /// <param name="requestUriString">The request URI.</param>
        /// <param name="routePrefix">Optional route prefix; default is 'fhir'.</param>
        public FhirPath(string method, string requestUriString, string routePrefix = "fhir")
            : base(requestUriString)
        {
            Method = method;
            RoutePrefix = routePrefix;
            SetPathParts(requestUriString, routePrefix);
        }

        /// <summary>
        /// Gets or sets the route prefix.
        /// </summary>
        public string RoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method used with the request URI.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the FHIR resource in the request URI.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the FHIR id in the request URI.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the FHIR operation in the request URI.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the FHIR version in the request URI.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets the path of the request Uri.
        /// </summary>
        public string Path
        {
            get
            {
                StringBuilder builder = new();

                if (!string.IsNullOrEmpty(RoutePrefix))
                {
                    builder = AddPathSegment(RoutePrefix, builder);
                }

                builder = AddPathSegment(Resource, builder);
                builder = AddPathSegment(Id, builder);
                builder = AddPathSegment(Operation, builder);
                builder = AddPathSegment(Version, builder);
                return builder.ToString().TrimEnd('/');
            }
        }

        /// <summary>
        /// Gets the path of the request URI exclusive on route prefix.
        /// </summary>
        public string NormalizedPath
        {
            get
            {
                StringBuilder builder = new();
                if (Resource != null)
                {
                    builder.Append(Resource);
                }

                if (Id != null)
                {
                    builder.Append($"/{Id}");
                }

                if (Operation != null)
                {
                    builder.Append($"/{Operation}");
                }

                if (Version != null)
                {
                    builder.Append($"/{Version}");
                }

                return builder.ToString().TrimEnd('/');
            }
        }

        /// <summary>
        /// Indicates whether a query string parameter is present in the request URI.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasQueryParameter(string key)
        {
            if (base.Query == null)
            {
                return false;
            }

            NameValueCollection query = HttpUtility.ParseQueryString(base.Query);
            return query.AllKeys.Any(str => str.ToLowerInvariant().Contains(key.ToLowerInvariant()));
        }

        private static StringBuilder AddPathSegment(string segment, StringBuilder builder)
        {
            if (!string.IsNullOrEmpty(segment))
            {
                builder.Append($"{segment}/");
            }

            return builder;
        }

        private void SetPathParts(string uriString, string routePrefix)
        {
            Uri uri = new Uri(uriString).RemoveRoutePrefix(routePrefix);

            var values = (from item in uri.Segments
                          where (item.Length > 0 && item != "/")
                          select item.Replace("/", ""));

            var parts = values.ToArray();
            Resource = parts.Length > 0 ? parts.ElementAt(0) : null;
            Id = parts.Length > 1 ? parts.ElementAt(1) : null;
            Operation = parts.Length > 2 ? parts.ElementAt(2) : null;
            Version = parts.Length > 3 ? parts.ElementAt(3) : null;
        }

    }
}
