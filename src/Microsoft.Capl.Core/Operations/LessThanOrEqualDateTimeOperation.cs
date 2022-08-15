using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two DateTime values to determine if the lhs is less than or equal the rhs (binding variable).
    /// </summary>
    [Serializable]
    [JsonObject("operation")]
    public class LessThanOrEqualDateTimeOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#LessThanOrEqualDateTime";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            DateTimeComparer comparer = new();
            int result = comparer.Compare(lhs, Value);
            return result == 0 || result == -1;
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
