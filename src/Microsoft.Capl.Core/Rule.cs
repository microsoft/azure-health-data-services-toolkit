using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Capl.Matching;
using Capl.Operations;
using Newtonsoft.Json;

namespace Capl
{
    [Serializable]
    [JsonObject("rule")]
    public class Rule : Term
    {
        public Rule()
        {

        }

        public Rule(bool eval = true)
            : this(eval, null, null)
        {

        }

        public Rule(bool eval, Match? matchExp, Operation? operationExp)
            : this(null, eval, matchExp, operationExp)
        {

        }

        public Rule([AllowNull] string id, bool eval, [AllowNull] Match matchExp, [AllowNull] Operation operationExp)
        {
            Id = id;
            Eval = eval;
            MatchExp = matchExp;
            OperationExp = operationExp;
        }

        private bool eval = true;


        [JsonProperty("eval")]
        public override bool Eval
        {
            get { return eval; }
            set { eval = value; }
        }

        [AllowNull]
        [JsonProperty("operation")]
        public Operation OperationExp { get; set; }

        [AllowNull]
        [JsonProperty("match")]
        public Match MatchExp { get; set; }

        [JsonProperty("type")]
        public override string Type => "#Rule";

        public override bool Evaluate(IEnumerable<Claim> claims)
        {
            _ = claims ?? throw new ArgumentNullException(nameof(claims));
            _ = OperationExp ?? throw new NullReferenceException(nameof(OperationExp));
            _ = MatchExp ?? throw new NullReferenceException(nameof(MatchExp));

            IList<Claim> claimSet = MatchExp.MatchClaims(claims);

            if (claimSet.Count == 0)
            {
                return !MatchExp.Required;
            }

            foreach (var claim in claimSet)
            {
                bool eval = OperationExp.Execute(claim.Value);

                if (Eval && eval)
                {
                    return true;
                }

                if (!Eval && eval)
                {
                    return false;
                }
            }

            return !Eval;
        }

        public override void Serialize(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("#Rule");

            writer.WritePropertyName("eval");
            writer.WriteValue(this.Eval);

            if (!string.IsNullOrEmpty(this.Id))
            {
                writer.WritePropertyName("id");
                writer.WriteValue(this.Id);
            }

            writer.WritePropertyName("match");
            serializer.Serialize(writer, this.MatchExp);

            writer.WritePropertyName("operation");
            serializer.Serialize(writer, this.OperationExp);

            writer.WriteEndObject();
        }
    }
}
