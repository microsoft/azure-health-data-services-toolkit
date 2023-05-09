using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using Microsoft.AzureHealth.DataServices.Configuration;

namespace Microsoft.AzureHealth.DataServices.Protocol
{
    /// <summary>
    /// FHIR URI path.
    /// </summary>
    public class FhirUriPath : Uri
    {
        /// <summary>
        /// Creates an instance of FhirPath.
        /// </summary>
        /// <param name="method">HTTP method used in request.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="routePrefix">Optional route prefix.</param>
        public FhirUriPath(HttpMethod method, Uri requestUri, string routePrefix = "")
            : this(method, requestUri.ToString(), routePrefix)
        { }

        /// <summary>
        /// Creates an instance of FhirPath.
        /// </summary>
        /// <param name="method">HTTP method used in request.</param>
        /// <param name="requestUriString">The request URI.</param>
        /// <param name="routePrefix">Optional route prefix.</param>
        public FhirUriPath(HttpMethod method, string requestUriString, string routePrefix = "")
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
        public HttpMethod Method { get; set; }

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
        public FhirOperationType? Operation { get; set; } = null;

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
                string rtn = "";
                if (!string.IsNullOrEmpty(RoutePrefix))
                {
                    rtn = $"{RoutePrefix}/";
                }

                return rtn + NormalizedPath;
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

                if (Operation is not null)
                {
                    if (Id is null)
                    {
                        AddPathSegment($"${Operation.GetDescription()}", builder);
                    }
                    else if (Operation.GetCategory() == "async" )
                    {
                        AddPathSegment("/_operations", builder);
                        AddPathSegment($"/{Operation.GetDescription()}", builder);
                        AddPathSegment($"/{Id}", builder);
                    }
                }

                if (Resource is not null)
                {
                    AddPathSegment(Resource, builder);
                    if (Id != null)
                    {
                        AddPathSegment($"/{Id}", builder);
                    }

                    if (Version is not null)
                    {
                        AddPathSegment("_history", builder);
                        AddPathSegment(Version, builder);
                    }
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

            // Handle root level operation= requests
            try
            {
                Operation = EnumExtensions.GetValueFromDescription<FhirOperationType>(values.ElementAt(0).TrimStart('$'));
                return;
            }
            catch { }
            

            // Handle operation instance requests
            if (values.ElementAt(0).Equals("_operations", StringComparison.CurrentCultureIgnoreCase))
            {
                Operation = EnumExtensions.GetValueFromDescription<FhirOperationType>(values.ElementAt(1));
                Id = values.ElementAt(2);
                return;
            }

            // Handle bundle operations
            if (!values.Any())
            {
                return;
            }

            // Handle resource requests
            Resource = values.ElementAt(0);
            Id = values.Count() > 1 ? values.ElementAt(1) : null;
            Version = values.Count()> 3 && string.Equals(values.ElementAt(2), "_history", StringComparison.CurrentCultureIgnoreCase) ? values.ElementAt(3) : null;
        }

    }
}
