using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Capl
{
    public abstract class JsonTermConverter<T> : JsonConverter
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
            _ = reader ?? throw new ArgumentNullException(nameof(reader));
            _ = objectType ?? throw new ArgumentNullException(nameof(objectType));
            _ = serializer ?? throw new ArgumentNullException(nameof(serializer));

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
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            _ = value ?? throw new ArgumentNullException(nameof(value));
            _ = serializer ?? throw new ArgumentNullException(nameof(serializer));

            Term term = (Term)value;
            term.Serialize(writer, serializer);
        }
    }
}
