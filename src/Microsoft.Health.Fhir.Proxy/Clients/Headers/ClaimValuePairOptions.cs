namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Options for configuring custom identity http headers.
    /// </summary>
    public class ClaimValuePairOptions
    {
        /// <summary>
        /// Gets or sets a custom header name.
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets a claim type in a security token to obtain a value.
        /// </summary>
        public string ClaimType { get; set; }
    }
}
