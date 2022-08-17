using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two decimals for equality.
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class EqualNumericOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#EqualNumeric";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");

            DecimalComparer comparer = new();
            return comparer.Compare(lhs, Value) == 0;
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
