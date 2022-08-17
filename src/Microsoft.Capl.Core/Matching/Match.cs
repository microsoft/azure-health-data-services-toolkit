using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Capl.Matching
{
    [Serializable]
    [JsonConverter(typeof(MatchConverter))]
    public abstract class Match
    {
        protected string type = "#Literal";
        protected bool required = true;

        [JsonProperty("type")]
        public virtual string Type
        {
            get { return type; }
            set
            {
                if (value == null)
                {
                    value = type;
                }

                type = value;
            }
        }

        [JsonProperty("required")]
        public virtual bool Required
        {
            get { return required; }
            set
            {
                required = value;
            }
        }

        [AllowNull]
        [JsonProperty("claimType")]
        public virtual string ClaimType { get; set; }

        [JsonProperty("value")]
        public abstract string Value { get; set; }

        public abstract IList<Claim> MatchClaims(IEnumerable<Claim> claims);

        public abstract void Serialize(JsonWriter writer);
    }
}
