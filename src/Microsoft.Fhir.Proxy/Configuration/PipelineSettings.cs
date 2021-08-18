using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Fhir.Proxy.Configuration
{
    public class PipelineSettings
    {
        public static PipelineSettings Default => new();

        public PipelineSettings()
        {
            FilterNames = new List<string>();
            ChannelNames = new List<string>();
        }

        public PipelineSettings(IEnumerable<string> filterNames, IEnumerable<string> channelNames)
        {
            FilterNames = filterNames != null ? new List<string>(filterNames) : new List<string>();
            ChannelNames = channelNames != null ? new List<string>(channelNames) : new List<string>();
        }

        [JsonProperty("filters")]
        public virtual List<string> FilterNames { get; set; }

        [JsonProperty("channels")]
        public virtual List<string> ChannelNames { get; set; }
    }
}
