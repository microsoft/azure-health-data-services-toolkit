namespace Microsoft.AzureHealth.DataServices.Clients.Headers
{
    /// <summary>
    /// A name value pair and type of header for the operation.
    /// </summary>
    public class HeaderNameValuePair : IHeaderNameValuePair
    {
        /// <summary>
        /// Creates an instance of HeaderNameValuePair.
        /// </summary>
        /// <param name="name">The name of pair.</param>
        /// <param name="value">The value of pair.</param>
        /// <param name="headerType">Type of header for the operation.</param>
        public HeaderNameValuePair(string name, string value, CustomHeaderType headerType)
        {
            Name = name;
            Value = value;
            HeaderType = headerType;
        }

        /// <summary>
        /// Gets or sets the name of the pair.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the pair.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of custom header for the operation.
        /// </summary>
        public CustomHeaderType HeaderType { get; set; }
    }
}
