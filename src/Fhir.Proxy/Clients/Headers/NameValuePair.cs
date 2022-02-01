using Microsoft.Extensions.Options;

namespace Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// A name value pair.
    /// </summary>
    public class NameValuePair : INameValuePair
    {
        /// <summary>
        /// Creates an instance of NameValuePair.
        /// </summary>
        /// <param name="options">Options that define the name value pair.</param>
        public NameValuePair(IOptions<NameValuePairOptions> options)
        {
            Name = options.Value.Name;
            Value = options.Value.Value;
        }

        /// <summary>
        /// Creates an instance of NameValuePair.
        /// </summary>
        /// <param name="name">The name of the pair.</param>
        /// <param name="value">The value of the pair.</param>
        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }

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
