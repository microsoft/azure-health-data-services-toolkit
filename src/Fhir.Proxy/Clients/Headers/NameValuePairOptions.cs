namespace Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Options for configuring  name value pairs.
    /// </summary>
    public class NameValuePairOptions
    {
        /// <summary>
        /// Gets or sets the name of the pair.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the pair.
        /// </summary>
        public string Value { get; set; }
    }
}
