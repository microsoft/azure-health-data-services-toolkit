using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    [Serializable]
    [JsonObject("operation")]
    public class BetweenExclusiveOperation : Operation
    {
        [JsonProperty("type")]
        public override string Type => "#BetweenExclusive";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            string[] parts = Value.Split(" ");
            DecimalComparer comparer = new();
            int p1 = comparer.Compare(lhs, parts[0]);
            int p2 = comparer.Compare(lhs, parts[1]);

            return p1 == 1 && p2 == -1;
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
