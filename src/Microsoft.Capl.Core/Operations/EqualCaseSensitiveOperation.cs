using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two strings for equality case sensitive;
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class EqualCaseSensitiveOperation : Operation
    {
        public EqualCaseSensitiveOperation()
        {
        }

        public EqualCaseSensitiveOperation(string value)
        {
            Value = value;
        }

        [JsonProperty("type")]
        public override string Type => "#EqualCaseSensitive";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");

            return Value == lhs;
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
