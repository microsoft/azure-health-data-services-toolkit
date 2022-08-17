using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Capl.Operations
{
    public abstract class JsonOperationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {

            var jObject = JObject.Load(reader);

            T target = Create(objectType, jObject);

            _ = target ?? throw new InvalidCastException(nameof(target));

            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(
        JsonWriter writer,
        object? value,
        JsonSerializer serializer)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));

            Operation op = (Operation)value;
            op.Serialize(writer);
        }
    }
}
