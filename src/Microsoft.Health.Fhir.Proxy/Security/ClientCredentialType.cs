namespace Microsoft.Health.Fhir.Proxy.Security
{
    public enum ClientCredentialType
    {
        ClientSecret,
        Certificate,
        ManagedIdentity,
        OnBehalfOfUsingClientSecert,
        OnBehalfOfUsingCertificate
    }
}
