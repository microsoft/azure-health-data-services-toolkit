using System;
using System.Net;

namespace Fhir.Proxy.Filters
{
    /// <summary>
    /// Filter error event arguments used when filters signal an error.
    /// </summary>
    public class FilterErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of FilterErrorEventArgs
        /// </summary>
        /// <param name="name">Name of the filter.</param>
        /// <param name="id">Instance id of the filter.</param>
        /// <param name="fatal">Indicator as to whether the filter has caught a fatal error.</param>
        /// <param name="error">An error caught by a filter.</param>
        /// <param name="code">Http response code, if applicable.</param>
        /// <param name="responseBody">Http response body, if applicable.</param>
        public FilterErrorEventArgs(string name, string id, bool fatal = false, Exception error = null, HttpStatusCode? code = null, string responseBody = null)
        {
            Name = name;
            Id = id;
            IsFatal = fatal;
            Error = error;
            Code = code;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the instance id of the filter.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets an indicator that determines if the filter has a fatal error.
        /// </summary>
        public bool IsFatal { get; private set; }

        /// <summary>
        /// Get the http status code for a response; otherwise null.
        /// </summary>
        public HttpStatusCode? Code { get; private set; }

        /// <summary>
        /// Gets an exception thrown by the filter; otherwise null.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a response body from a request; otherwise null.
        /// </summary>
        public string ResponseBody { get; private set; }
    }
}
