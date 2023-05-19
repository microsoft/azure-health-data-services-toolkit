using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class LargeJsonMessage
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly List<string> _values;

        public LargeJsonMessage()
        {
            _values = new List<string>();
            Fields = new List<KeyValuePair<string, string>>();
        }

        [JsonProperty("fields")]
        public List<KeyValuePair<string, string>> Fields { get; set; }

        public void Load(int fields, int totalSizeBytes)
        {
            int length = totalSizeBytes / fields;

            var random = new Random();

            for (int i = 0; i < fields; i++)
            {
                var randomString = new string(Enumerable.Repeat(Chars, length)
                                                        .Select(s => s[random.Next(s.Length)]).ToArray());
                _values.Add(randomString);
            }

            for (int i = 0; i < _values.Count; i++)
            {
                Fields.Add(new KeyValuePair<string, string>($"field{i}", _values[i]));
            }
        }
    }
}
