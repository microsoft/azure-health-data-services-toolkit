namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public interface INameValuePair
    {
        string Name { get; set; }

        string Value { get; set; }
    }
}
