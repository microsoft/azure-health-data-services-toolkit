using System;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace DataServices.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class MyEntity : ITableEntity
    {
        public MyEntity()
        {
        }

        public MyEntity(string key, string messageId, string firstName, string lastName)
        {
            Key = key;
            MessageId = messageId;
            FirstName = firstName;
            LastName = lastName;
        }

        [JsonProperty("key")]
        public string Key
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        [JsonProperty("messageId")]
        public string MessageId
        {
            get => RowKey;
            set => RowKey = value;
        }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
