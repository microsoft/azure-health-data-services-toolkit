namespace Azure.Health.DataServices.Security
{
    /// <summary>
    /// Client credential type use when acquiring access tokens.
    /// </summary>
    public enum ClientCredentialType
    {
        /// <summary>
        /// Client secret.
        /// </summary>
        ClientSecret,

        /// <summary>
        /// X509 certificate.
        /// </summary>
        Certificate,

        /// <summary>
        /// Managed identity.
        /// </summary>
        ManagedIdentity,

        /// <summary>
        /// OnBehalfOf using a client secret.
        /// </summary>
        OnBehalfOfUsingClientSecert,

        /// <summary>
        /// OnBehalfOf useing a certificate.
        /// </summary>
        OnBehalfOfUsingCertificate
    }
}
