using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Capl.Operations
{
    [Serializable]
    [JsonObject("operation")]
    public class BetweenDateTimeOperation : Operation
    {
        public BetweenDateTimeOperation()
        {
        }

        public BetweenDateTimeOperation(string lhs)
            : base(lhs)
        {

        }

        [JsonProperty("type")]
        public override string Type => "#BetweenDateTime";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override bool Execute(string lhs)
        {
            ///the lhs is ignored and the RHS uses a normalized string containing 2 C# partable dateTime values.
            ///the current time should be between the dateTime values

            _ = Value ?? throw new InvalidOperationException("RHS Value cannot be null.");


            string[] parts = Value.Split(new[] { ' ' });
            DateTime dateTime1 = DateTime.Parse(parts[0]);
            DateTime dateTime2 = DateTime.Parse(parts[1]);
            DateTime now = DateTime.Now;

            if (dateTime1 < dateTime2)
            {
                return dateTime1 <= now && dateTime2 >= now;
            }
            else
            {
                return dateTime2 <= now && dateTime1 >= now;
            }

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
