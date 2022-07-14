namespace DataServices.Security
{
    /// <summary>
    /// Client credential type use when acquiring access tokens.
    /// </summary>
    public enum ClientCredentialType
    {
        ClientSecret,
        Certificate,
        ManagedIdentity,
        OnBehalfOfUsingClientSecert,
        OnBehalfOfUsingCertificate
    }
}
