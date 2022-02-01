namespace Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Interface for implementing a name value pair.
    /// </summary>
    public interface INameValuePair
    {
        /// <summary>
        /// Gets or sets a name of the pair.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value of the pair.
        /// </summary>
        string Value { get; set; }
    }
}
