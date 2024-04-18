using System;
using System.Runtime.Serialization;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Service Bus Channel exception.
    /// </summary>
    public class ServiceBusChannelException : Exception
    {
        /// <summary>
        /// Creates an instance of ServiceBusChannelException.
        /// </summary>
        public ServiceBusChannelException()
        {
        }

        /// <summary>
        /// Creates an instance of ServiceBusChannelException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public ServiceBusChannelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of ServiceBusChannelException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public ServiceBusChannelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of ServiceBusChannelException.
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected ServiceBusChannelException(SerializationInfo info, StreamingContext context)
       : base(info.GetString("Message"), (Exception)info.GetValue("InnerException", typeof(Exception)))
        {
        }
    }
}
