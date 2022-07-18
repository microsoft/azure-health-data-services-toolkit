using Newtonsoft.Json.Linq;
using System.Runtime.Remoting;

namespace Capl.Operations
{
    public class OperationConverter : JsonOperationConverter<Operation>
    {
        private const JTokenType TokenType = JTokenType.String;
        private const string TypePropertyName = "type";

        protected override Operation Create(Type objectType, JObject jObject)
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
                    ObjectHandle? handle = Activator.CreateInstance("Capl", $"Capl.Operations.{id.TrimStart('#')}Operation");
                    if (handle?.Unwrap() is not Operation operation)
                    {
                        throw new ArgumentException("operation");
                    }
                    else
                    {
                        return operation;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Operation type.");
            }
        }
    }
}
