using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    [Serializable]
    [JsonObject("operation")]
    public class NotEqualCaseInsensitiveOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#NotEqualCaseInsenitive";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            return Value.ToLowerInvariant() != lhs.ToLowerInvariant();
        }

        public override void Serialize(JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue(this.Type);

            writer.WritePropertyName("value");
            if (string.IsNullOrEmpty(this.Value))
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(this.Value);
            }

            writer.WriteEndObject();
        }
    }
}
