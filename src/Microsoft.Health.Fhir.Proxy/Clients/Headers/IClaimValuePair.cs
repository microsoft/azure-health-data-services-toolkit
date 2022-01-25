namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public interface IClaimValuePair
    {
        string HeaderName { get; set; }

        string ClaimType { get; set; }
    }
}
