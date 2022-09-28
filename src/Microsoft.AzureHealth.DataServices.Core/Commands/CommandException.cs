using System;
using System.Runtime.Serialization;

namespace Microsoft.AzureHealth.DataServices.Commands
{
    /// <summary>
    /// Command exception.
    /// </summary>
    public class CommandException : Exception
    {
        /// <summary>
        /// Creates an instance of CommandException.
        /// </summary>
        public CommandException()
        {

        }

        /// <summary>
        /// Creates an instance of CommandException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public CommandException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates an instance of CommandException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates an instance of CommandException.
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">StreamingContext</param>
        protected CommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
