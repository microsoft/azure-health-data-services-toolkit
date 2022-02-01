namespace Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Interface for implementing a claim value pair.
    /// </summary>
    public interface IClaimValuePair
    {
        /// <summary>
        /// Gets or sets a custom header name.
        /// </summary>
        string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets a claim type in a security token to obtain a value.
        /// </summary>
        string ClaimType { get; set; }
    }
}
