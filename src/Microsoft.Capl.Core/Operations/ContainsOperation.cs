using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Capl.Operations
{
    [Serializable]
    [JsonObject("operation")]
    public class ContainsOperation : Operation
    {
        public ContainsOperation()
        {
        }

        public ContainsOperation(string value)
            : base(value)
        {
        }

        [JsonProperty("type")]
        public override string Type => "#Contains";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");

            return Value.Contains(lhs);
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
