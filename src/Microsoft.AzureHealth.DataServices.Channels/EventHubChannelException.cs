using System;
using System.Runtime.Serialization;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Event Hub Channel exception.
    /// </summary>
    public class EventHubChannelException : Exception
    {
        /// <summary>
        /// Creates an instance of EventHubChannelException.
        /// </summary>
        public EventHubChannelException()
        {
        }

        /// <summary>
        /// Creates an instance of EventHubChannelException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public EventHubChannelException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates an instance of EventHubChannelException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public EventHubChannelException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates an instance of EventHubChannelException.
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected EventHubChannelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
