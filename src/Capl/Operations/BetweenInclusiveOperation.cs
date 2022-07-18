using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Capl.Operations
{
    [Serializable]
    [JsonObject("operation")]
    public class BetweenInclusiveOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#BetweenInclusive";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");

            string[] parts = Value.Split(" ");
            DecimalComparer comparer = new();
            int p1 = comparer.Compare(lhs, parts[0]);
            int p2 = comparer.Compare(lhs, parts[1]);

            return (p1 == 1 || p1 == 0) && (p2 == -1 || p2 == 0);

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
