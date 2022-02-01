using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Fhir.Proxy.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class MyEntity : TableEntity
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
    }
}
