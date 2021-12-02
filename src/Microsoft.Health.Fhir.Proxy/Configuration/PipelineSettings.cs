using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    /// <summary>
    /// Settings for pipelines.
    /// </summary>
    public class PipelineSettings
    {
        /// <summary>
        /// Default PipelineSettings instance.
        /// </summary>
        public static PipelineSettings Default => new();

        /// <summary>
        /// Creates a new instance of PipelineSettings.
        /// </summary>
        public PipelineSettings()
        {
            FilterNames = new List<string>();
            ChannelNames = new List<string>();
        }

        /// <summary>
        /// Creates a new instance of PipelineSettings.
        /// </summary>
        /// <param name="pipelineName">Name of pipeline to execute.</param>
        /// <param name="filterNames">Names of filters implemented by pipeline.</param>
        /// <param name="channelNames">Names of channels implemented by pipeline.</param>
        public PipelineSettings(string pipelineName, IEnumerable<string> filterNames, IEnumerable<string> channelNames)
        {
            Name = pipelineName;
            FilterNames = filterNames != null ? new List<string>(filterNames) : new List<string>();
            ChannelNames = channelNames != null ? new List<string>(channelNames) : new List<string>();
        }

        /// <summary>
        /// Gets or stes the name of the pipeline implemented.
        /// </summary>

        [JsonProperty("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets a list of filters implemented by the pipeline.
        /// </summary>
        [JsonProperty("filters")]
        public virtual List<string> FilterNames { get; set; }

        /// <summary>
        /// Gets or sets a list of channels implemented by the pipeline.
        /// </summary>
        [JsonProperty("channels")]
        public virtual List<string> ChannelNames { get; set; }
    }
}
