using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    public class LoggingConfig
    {
        /// <summary>
        /// Gets or sets the instrumentation key used for app insights.
        /// </summary>
        [JsonProperty("instrumentationKey")]
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Gets or sets the log level used for app insights.
        /// </summary>
        [JsonProperty("logLevel")]
        public LogLevel LoggingLevel { get; set; }
    }
}
