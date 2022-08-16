using System.Runtime.Remoting;
using Newtonsoft.Json.Linq;

namespace Capl.Matching
{
    public class MatchConverter : JsonMatchConverter<Match>
    {
        private const JTokenType TokenType = JTokenType.String;
        private const string TypePropertyName = "type";
        private const string ClaimTypePropertyName = "claimType";

        protected override Match Create(Type objectType, JObject jObject)
        {
            _ = objectType ?? throw new ArgumentNullException(nameof(objectType));
            _ = jObject ?? throw new ArgumentNullException(nameof(jObject));

            jObject.TryGetValue(TypePropertyName, out JToken? typeToken);
            jObject.TryGetValue(ClaimTypePropertyName, out JToken? claimTypeToken);

            if (typeToken != null && typeToken.Type == TokenType)
            {
                string? id = jObject?[TypePropertyName]?.ToString();

                if (id == null || id.Length == 0)
                {
                    throw new ArgumentException("type");
                }
                else
                {
                    ObjectHandle? handle = Activator.CreateInstance("Microsoft.Capl.Core", $"Capl.Matching.{id.TrimStart('#')}Match");
                    if (handle?.Unwrap() is not Match match)
                    {
                        throw new ArgumentException("match");
                    }
                    else
                    {
                        return match;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Match type.");
            }
        }
    }
}
