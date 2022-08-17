using System.Runtime.Remoting;
using Newtonsoft.Json.Linq;

namespace Capl
{
    public class TermConverter : JsonTermConverter<Term>
    {
        private const JTokenType TokenType = JTokenType.String;
        private const string TypePropertyName = "type";

        protected override Term Create(Type objectType, JObject jObject)
        {
            _ = objectType ?? throw new ArgumentNullException(nameof(objectType));
            _ = jObject ?? throw new ArgumentNullException(nameof(jObject));

            jObject.TryGetValue(TypePropertyName, out JToken? typeToken);

            if (typeToken != null && typeToken.Type == TokenType)
            {
                string? id = jObject?[TypePropertyName]?.ToString();

                if (id == null || id.Length == 0)
                {
                    throw new ArgumentException("type");
                }
                else
                {
                    ObjectHandle? handle = Activator.CreateInstance("Microsoft.Capl.Core", $"Capl.{id.TrimStart('#')}");
                    if (handle?.Unwrap() is not Term term)
                    {
                        throw new ArgumentException("term");
                    }
                    else
                    {
                        return term;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Term type.");
            }
        }
    }
}
