namespace Quickstart.Configuration
{
    public class EventInputBinding
    {
        public string Id { get; set; }

        public string Topic { get; set; }

        public string Subject { get; set; }

        public string EventType { get; set; }

        public DateTime EventTime { get; set; }

        public IDictionary<string, object> Data { get; set; }
    }
}