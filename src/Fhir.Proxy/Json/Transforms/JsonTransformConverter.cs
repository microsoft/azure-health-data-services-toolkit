using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json.Transforms
{
    /// <summary>
    /// JSON.NET transform converter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JsonTransformConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        /// <summary>
        /// Indicates whether the object can be converter.
        /// </summary>
        /// <param name="objectType">Type of object to convert.</param>
        /// <returns>True if can be converted; otherwise false.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Read the json object.
        /// </summary>
        /// <param name="reader">JsonReader</param>
        /// <param name="objectType">Type of object to read.</param>
        /// <param name="existingValue">Object to read.</param>
        /// <param name="serializer">JsonSerializer.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Write json transform.
        /// </summary>
        /// <param name="writer">JsonWriter</param>
        /// <param name="value">Object to write.</param>
        /// <param name="serializer">JsonSerializer</param>
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
                    throw new ArgumentOutOfRangeException(nameof(value), $"Not expected type value: {transform.Name}");
            }
        }

        /// <summary>
        /// Indicates whether a field exists.
        /// </summary>
        /// <param name="jObject">JObject to evaluate.</param>
        /// <param name="name">Name of field.</param>
        /// <param name="type">Type of JToken.</param>
        /// <returns></returns>
        protected static bool FieldExists(
            JObject jObject,
            string name,
            JTokenType type)
        {
            return jObject.TryGetValue(name, out JToken token) && token.Type == type;
        }

        private static void SerializeAddTransform(JsonWriter writer, Transform transform)
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

        private static void SerializeRemoveTransform(JsonWriter writer, Transform transform)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(transform.Name);

            writer.WritePropertyName("jsonPath");
            writer.WriteValue(transform.JsonPath);

            writer.WriteEndObject();
        }

        private static void SerializeReplaceTransform(JsonWriter writer, Transform transform)
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
