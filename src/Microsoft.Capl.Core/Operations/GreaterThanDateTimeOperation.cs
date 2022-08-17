using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two DateTime values to determine if the lhs is greater than the rhs (binding variable).
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class GreaterThanDateTimeOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#GreaterThanDateTime";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            DateTimeComparer comparer = new();
            return comparer.Compare(lhs, Value) == 1;
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
