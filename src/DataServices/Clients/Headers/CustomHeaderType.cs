namespace Azure.Health.DataServices.Clients.Headers
{
    /// <summary>namespace Fhir.Proxy.Clients
    /// Type of custom header to inject.
    /// </summary>
    public enum CustomHeaderType
    {
        /// <summary>
        /// Injects a new header name and value that is determined the name value pair.
        /// </summary>
        /// <remarks>This is statically defined name and value of the header.</remarks>
        Static,
        /// <summary>
        /// Injects a new header name with the value determined by a claim type in the security token.
        /// </summary>
        /// <remarks>The name paramter is the name of the new http header and the value is a claim type in the security token, which its value is used in the new header.</remarks>
        Identity,
        /// <summary>
        /// Injecta new header name with the value determined by header in an incoming http request.
        /// </summary>
        /// <remarks>The name parameter is the nbame of the new http header and the value is name of a header in an incoming http request, which its value is used in the new header.</remarks>
        Request
    }
}
