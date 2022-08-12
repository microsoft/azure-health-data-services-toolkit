using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Capl
{
    [Serializable]
    [JsonConverter(typeof(TermConverter))]
    public abstract class Term
    {

        [AllowNull]
        [JsonProperty("id")]
        public virtual string Id
        {
            get; set;
        }

        [JsonProperty("type")]
        public abstract string Type { get; }

        [JsonProperty("eval")]
        public abstract bool Eval
        {
            get; set;
        }


        public abstract bool Evaluate(IEnumerable<Claim> claims);


        public abstract void Serialize(JsonWriter writer, JsonSerializer serializer);
    }
}
