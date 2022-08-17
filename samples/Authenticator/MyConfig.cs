namespace AuthenticatorSample
{
    // This class holds the configuration for the application that
    // needs to be set at runtime.
    public class MyConfig
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string FhirServerUrl { get; set; }
    }
}
