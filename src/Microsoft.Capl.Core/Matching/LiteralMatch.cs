using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Capl.Matching
{
    [Serializable]
    [JsonObject("match")]
    public class LiteralMatch : Match
    {
        public LiteralMatch()
        {
        }

        public LiteralMatch(string claimType, bool required = true)
            : this(claimType, null, required)
        {
        }

        public LiteralMatch(string claimType, string? value = null, bool required = true)
        {
            ClaimType = claimType;
            Value = value;
            Required = required;
        }

        [JsonProperty("type")]
        public override string Type => "#Literal";


        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override IList<Claim> MatchClaims(IEnumerable<Claim> claims)
        {
            _ = claims ?? throw new ArgumentNullException(nameof(claims));

            ClaimsIdentity ci = new(claims);
            IEnumerable<Claim> claimSet = ci.FindAll(delegate (Claim claim)
            {
                if (Value == null)
                {
                    return claim.Type == ClaimType;
                }

                return claim.Type == ClaimType && claim.Value == Value;
            });

            return new List<Claim>(claimSet);
        }

        public override void Serialize(JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue(this.Type);

            writer.WritePropertyName("required");
            writer.WriteValue(this.Required);

            writer.WritePropertyName("claimType");
            writer.WriteValue(this.ClaimType);


            if (!string.IsNullOrEmpty(this.Value))
            {
                writer.WritePropertyName("value");
                writer.WriteValue(this.Value);
            }

            writer.WriteEndObject();
        }

    }
}
