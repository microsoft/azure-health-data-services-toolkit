using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    public abstract class JsonTransformConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            T target = Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(
        JsonWriter writer,
        object value,
        JsonSerializer serializer)
        {
            Transform transform = (Transform)value;

            switch (transform.Name)
            {
                case "add":
                    SerializeAddTransform(writer, transform);
                    break;
                case "remove":
                    SerializeRemoveTransform(writer, transform);
                    break;
                case "replace":
                    SerializeReplaceTransform(writer, transform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transform.Name), $"Not expected type value: {transform.Name}");
            }
        }

        protected static bool FieldExists(
            JObject jObject,
            string name,
            JTokenType type)
        {
            return jObject.TryGetValue(name, out JToken token) && token.Type == type;
        }

        private void SerializeAddTransform(JsonWriter writer, Transform transform)
        {
            AddTransform addTrans = (AddTransform)transform;

            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(transform.Name);

            writer.WritePropertyName("jsonPath");
            writer.WriteValue(transform.JsonPath);

            writer.WritePropertyName("appendNode");
            writer.WriteValue(addTrans.AppendNode);

            writer.WriteEndObject();
        }

        private void SerializeRemoveTransform(JsonWriter writer, Transform transform)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(transform.Name);

            writer.WritePropertyName("jsonPath");
            writer.WriteValue(transform.JsonPath);

            writer.WriteEndObject();
        }

        private void SerializeReplaceTransform(JsonWriter writer, Transform transform)
        {
            ReplaceTransform replaceTrans = (ReplaceTransform)transform;

            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(transform.Name);

            writer.WritePropertyName("jsonPath");
            writer.WriteValue(transform.JsonPath);

            writer.WritePropertyName("replaceNode");
            writer.WriteValue(replaceTrans.ReplaceNode);

            writer.WriteEndObject();
        }


    }
}
