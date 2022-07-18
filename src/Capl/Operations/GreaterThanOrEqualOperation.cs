using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two decimal values to determine if the left argument is greater than or equal the right argument.
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class GreaterThanOrEqualOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#GreateThanOrEqual";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            DecimalComparer comparer = new();
            int result = comparer.Compare(lhs, Value);
            return result == 0 || result == 1;
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
