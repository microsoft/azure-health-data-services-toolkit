using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    public class TransformConverter : JsonTransformConverter<Transform>
    {
        protected override Transform Create(Type objectType, JObject jObject)
        {
            if (FieldExists(jObject, "name", JTokenType.String))
            {
                string id = (string)jObject["name"];
                return id switch
                {
                    "add" => new AddTransform(),
                    "remove" => new RemoveTransform(),
                    "replace" => new ReplaceTransform(),
                    _ => throw new ArgumentOutOfRangeException($"Not expected type value: {id}"),
                };
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
