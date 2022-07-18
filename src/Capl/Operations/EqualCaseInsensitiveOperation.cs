using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two strings for equality case insensitive;
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class EqualCaseInsensitiveOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#EqualCaseInsensitive";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");

            return Value.ToLowerInvariant() == lhs.ToLowerInvariant();
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
