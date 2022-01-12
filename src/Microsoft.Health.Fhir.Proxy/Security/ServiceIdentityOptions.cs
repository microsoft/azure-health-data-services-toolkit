using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Security
{
    /// <summary>
    /// Service identity options for acquiring access tokens.
    /// </summary>
    public class ServiceIdentityOptions
    {
        /// <summary>
        /// Gets or sets AAD client id for authentication.
        /// </summary>
        /// /// <remarks>Property can be omitted with using either (i) MSI or (ii) X509 certificate for authentication</remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets AAD client secret for authentication.
        /// </summary>
        /// /// <remarks>Property can be omitted with using either (i) MSI or (ii) X509 certificate for authentication</remarks>    
        public string ClientSecret { get; set; }

       
        /// <summary>
        /// Gets or sets an X509v3 certificate used for client identity.
        /// </summary>
        public X509Certificate2 Certficate { get; set; }

        /// <summary>
        /// Gets or sets the type of client credential.
        /// </summary>
        public ClientCredentialType CredentialType { get; set; }

        /// <summary>
        /// Gets or sets Tenant ID used for AAD authentication.
        /// </summary>        
        public string TenantId { get; set; }

    }
}
