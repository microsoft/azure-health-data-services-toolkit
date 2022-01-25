using Microsoft.Extensions.Options;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public class ClaimValuePair : IClaimValuePair
    {
        public ClaimValuePair(IOptions<ClaimValuePairOptions> options)
        {
            HeaderName = options.Value.HeaderName;
            ClaimType = options.Value.ClaimType;
        }

        public ClaimValuePair(string headerName, string claimType)
        {
            HeaderName = headerName;
            ClaimType = claimType;
        }

        public string HeaderName { get; set; }  

        public string ClaimType { get; set; }
    }
}
