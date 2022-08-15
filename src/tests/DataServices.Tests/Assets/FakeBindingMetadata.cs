using Microsoft.Azure.Functions.Worker;

namespace Azure.Health.DataServices.Tests.Assets
{
    public class FakeBindingMetadata : BindingMetadata
    {
        public FakeBindingMetadata(string name, string type, BindingDirection direction)
        {
            Name = name;
            Type = type;
            Direction = direction;
        }

        public override string Type { get; }

        public override BindingDirection Direction { get; }

        public override string Name { get; }
    }
}
