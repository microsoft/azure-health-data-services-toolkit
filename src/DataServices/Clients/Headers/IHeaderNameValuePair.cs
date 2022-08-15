namespace Azure.Health.DataServices.Clients.Headers
{
    /// <summary>
    /// Interface for implementing a name value pair and type of custom header.
    /// </summary>
    public interface IHeaderNameValuePair
    {
        /// <summary>
        /// Gets or sets a name of the pair.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value of the pair.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of custom header.
        /// </summary>
        CustomHeaderType HeaderType { get; set; }
    }
}
