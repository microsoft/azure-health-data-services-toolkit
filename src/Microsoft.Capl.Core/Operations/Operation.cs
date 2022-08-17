using Newtonsoft.Json;

namespace Capl.Operations
{
    [Serializable]
    [JsonConverter(typeof(OperationConverter))]
    public abstract class Operation
    {
        protected Operation()
        {
        }

        protected Operation(string value)
        {
            Value = value;
        }
        public abstract string Type { get; }

        public abstract string Value { get; set; }

        public abstract bool Execute(string lhs);

        public abstract void Serialize(JsonWriter writer);
    }
}
