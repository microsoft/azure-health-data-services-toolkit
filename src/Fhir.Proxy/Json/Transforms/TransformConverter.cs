using System;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json.Transforms
{
    /// <summary>
    /// JSON.NET transform converter
    /// </summary>
    public class TransformConverter : JsonTransformConverter<Transform>
    {
        /// <summary>
        /// Creates a concrete transform and returns as abstract transform type.
        /// </summary>
        /// <param name="objectType">Transform object type.</param>
        /// <param name="jObject">Transform object.</param>
        /// <returns></returns>
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
