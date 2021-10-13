using Newtonsoft.Json;
using System;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration
{
    [Serializable]
    [JsonObject]
    public class EventGridConfig
    {
        public EventGridConfig()
        {

        }

        public EventGridConfig(string topicUriString, string topicAccessKey,
                               string subject, string eventType, string dataVersion)
        {
            EventGridTopicUriString = topicUriString;
            EventGridTopicAccessKey = topicAccessKey;
            EventGridSubject = subject;
            EventGridEventType = eventType;
            EventGridDataVersion = dataVersion;
        }


        [JsonProperty("topicUriString")]
        public string EventGridTopicUriString { get; set; }

        [JsonProperty("topicAccessKey")]
        public string EventGridTopicAccessKey { get; set; }

        [JsonProperty("subject")]
        public string EventGridSubject { get; set; }

        [JsonProperty("eventType")]
        public string EventGridEventType { get; set; }

        [JsonProperty("dataVersion")]
        public string EventGridDataVersion { get; set; }

        [JsonProperty("eventGridBlobConnectionString")]
        public string EventGridBlobConnectionString { get; set; }

        [JsonProperty("eventGridBlobContainer")]
        public string EventGridBlobContainer { get; set; }
    }
}
