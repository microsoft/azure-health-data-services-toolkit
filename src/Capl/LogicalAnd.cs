using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Capl
{
    [Serializable]
    [JsonObject("logicalAnd")]
    public class LogicalAnd : Term
    {
        public LogicalAnd()
        {

        }

        public LogicalAnd(bool eval = true)
        {
            Eval = eval;
        }

        public LogicalAnd(bool eval, IEnumerable<Term> terms)
            : this(null, eval, terms)
        {

        }

        public LogicalAnd(string? id, bool eval, IEnumerable<Term> terms)
        {
            Id = id;
            Eval = eval;
            Terms = new List<Term>(terms);
        }

        [JsonProperty("type")]
        public override string Type => "#LogicalAnd";

        [JsonProperty("eval")]
        public override bool Eval { get; set; }

        [AllowNull]
        [JsonProperty("terms")]
        public IList<Term> Terms { get; set; }


        public override bool Evaluate(IEnumerable<Claim> claims)
        {
            _ = claims ?? throw new ArgumentNullException(nameof(claims));

            foreach (Term item in Terms)
            {
                bool eval = item.Evaluate(claims);
                if (!eval)
                {
                    if (!Eval)
                    {
                        return true;
                    }

                    return false;
                }
            }

            return Eval;
        }

        public override void Serialize(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("#LogicalAnd");

            writer.WritePropertyName("eval");
            writer.WriteValue(this.Eval);

            if (!string.IsNullOrEmpty(this.Id))
            {
                writer.WritePropertyName("id");
                writer.WriteValue(this.Id);
            }

            writer.WritePropertyName("terms");
            serializer.Serialize(writer, this.Terms);

            writer.WriteEndObject();
        }
    }
}
