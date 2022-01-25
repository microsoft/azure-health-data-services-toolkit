using Microsoft.Extensions.Options;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public class NameValuePair : INameValuePair
    {
        public NameValuePair(IOptions<NameValuePairOptions> options)
        {
            Name = options.Value.Name;
            Value = options.Value.Value;
        }

        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
