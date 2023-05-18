using System;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    /// <summary>
    /// Configuration for Event Grid channel.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class EventGridConfig
    {
        /// <summary>
        /// Creates an instance of EventGridConfig.
        /// </summary>
        public EventGridConfig()
        {
        }

        /// <summary>
        /// Creates an instance of EventGridConfig.
        /// </summary>
        /// <param name="topicUriString">Event Grid topic URI string.</param>
        /// <param name="topicAccessKey">Event Grid topic access key.</param>
        /// <param name="subject">Event Grid subject.</param>
        /// <param name="eventType">Event Grid event type.</param>
        /// <param name="dataVersion">Event Grid data version.</param>
        public EventGridConfig(string topicUriString, string topicAccessKey, string subject, string eventType, string dataVersion)
        {
            EventGridTopicUriString = topicUriString;
            EventGridTopicAccessKey = topicAccessKey;
            EventGridSubject = subject;
            EventGridEventType = eventType;
            EventGridDataVersion = dataVersion;
        }

        /// <summary>
        /// Gets or sets Event Grid topic URI string.
        /// </summary>
        [JsonProperty("topicUriString")]
        public string EventGridTopicUriString { get; set; }

        /// <summary>
        /// Gets or sets Event Grid topic access key.
        /// </summary>
        [JsonProperty("topicAccessKey")]
        public string EventGridTopicAccessKey { get; set; }

        [JsonProperty("subject")]
        public string EventGridSubject { get; set; }

        /// <summary>
        /// Gets or sets Event Grid event type.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventGridEventType { get; set; }

        /// <summary>
        /// Gets or sets Event Grid data version.
        /// </summary>
        [JsonProperty("dataVersion")]
        public string EventGridDataVersion { get; set; }

        /// <summary>
        /// Gets or sets Azure storage connection string for managing large files.
        /// </summary>
        [JsonProperty("eventGridBlobConnectionString")]
        public string EventGridBlobConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Azure blob storage container name for managing large files.
        /// </summary>
        [JsonProperty("eventGridBlobContainer")]
        public string EventGridBlobContainer { get; set; }
    }
}
