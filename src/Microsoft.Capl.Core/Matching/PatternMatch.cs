using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Capl.Matching
{
    [Serializable]
    [JsonObject("match")]
    public class PatternMatch : Match
    {
        public PatternMatch()
        {
        }


        public PatternMatch(string claimType, string pattern, bool required = true)
        {
            ClaimType = claimType;
            Value = pattern;
            Required = required;
        }

        [JsonProperty("type")]
        public override string Type => "#Pattern";

        [AllowNull]
        [JsonProperty("value")]
        public override string Value { get; set; }

        public override IList<Claim> MatchClaims(IEnumerable<Claim> claims)
        {
            _ = claims ?? throw new ArgumentNullException(nameof(claims));

            if (Value == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (ClaimType == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            Regex regex = new(Value);

            ClaimsIdentity ci = new(claims);
            IEnumerable<Claim> claimSet = ci.FindAll(delegate (Claim claim)
            {
                return ClaimType == claim.Type;
            });

            if (Value == null)
            {
                return new List<Claim>(claimSet);
            }

            List<Claim> claimList = new();
            IEnumerator<Claim> en = claimSet.GetEnumerator();

            while (en.MoveNext())
            {
                if (regex.IsMatch(en.Current.Value))
                {
                    claimList.Add(en.Current);
                }
            }

            return claimList;
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
