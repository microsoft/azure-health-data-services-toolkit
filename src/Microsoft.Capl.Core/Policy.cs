using System.Security.Claims;
using Newtonsoft.Json;

namespace Capl
{
    [Serializable]
    [JsonObject("policy")]
    public class Policy
    {
        public Policy(string? id, Term expression)
        {
            Id = id;
            EvaluationExp = expression;
        }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("expression")]
        public Term EvaluationExp { get; set; }

        public bool Evaluate(IEnumerable<Claim> claimSet)
        {
            return EvaluationExp.Evaluate(claimSet);
        }
    }
}
