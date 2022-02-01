using System;
using Newtonsoft.Json;

namespace Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// A reference to a message stored as a file in blob storage.
    /// </summary>
    /// <remarks>This channel provides a reference to a message when using when the primary channel cannot accept a message by value due to its large size.</remarks>
    [Serializable]
    [JsonObject]
    public class EventDataByReference
    {
        /// <summary>
        /// Creates an instance of EventDataByReference.
        /// </summary>
        public EventDataByReference()
        {
        }

        /// <summary>
        /// Creates an instance of EventDataByReference.
        /// </summary>
        /// <param name="container">Blob container name where the file is stored.</param>
        /// <param name="blob">Filename containing the message.</param>
        /// <param name="contentType">Content type of the message.</param>
        public EventDataByReference(string container, string blob, string contentType)
        {
            this.Container = container;
            this.Blob = blob;
            this.ContentType = contentType;
        }

        /// <summary>
        /// Gets or sets the blob storage container name where the file is stored.
        /// </summary>
        [JsonProperty("container")]
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the filename of the message.
        /// </summary>
        [JsonProperty("blob")]
        public string Blob { get; set; }

        /// <summary>
        /// Gets or sets the content type of the message.
        /// </summary>
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
    }
}
