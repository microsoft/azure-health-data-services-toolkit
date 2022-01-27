using Microsoft.Extensions.Options;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Maps a custom header name to a claim type in a security token.
    /// </summary>
    public class ClaimValuePair : IClaimValuePair
    {
        /// <summary>
        /// Creates an instance of ClaimValuePair.
        /// </summary>
        /// <param name="options">Options that define the claim value pair.</param>
        public ClaimValuePair(IOptions<ClaimValuePairOptions> options)
        {
            HeaderName = options.Value.HeaderName;
            ClaimType = options.Value.ClaimType;
        }

        /// <summary>
        /// Creates an instance of ClaimValuePair.
        /// </summary>
        /// <param name="headerName">Custom HTTP header name.</param>
        /// <param name="claimType">Claim type in security token to obtain value.</param>
        public ClaimValuePair(string headerName, string claimType)
        {
            HeaderName = headerName;
            ClaimType = claimType;
        }

        /// <summary>
        /// Gets or sets the custom header name.
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets the claim type in the security token to obtain a value.
        /// </summary>
        public string ClaimType { get; set; }
    }
}
