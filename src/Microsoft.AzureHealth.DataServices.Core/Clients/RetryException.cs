using System;
using System.Runtime.Serialization;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Retry exception.
    /// </summary>
    public class RetryException : Exception
    {
        /// <summary>
        /// Creates an instance of RetryException.
        /// </summary>
        public RetryException()
        {
        }

        /// <summary>
        /// Creates an instance of RetryException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RetryException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates an instance of RetryException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public RetryException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates an instance of RetryException.
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected RetryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
