using System;
using System.Runtime.Serialization;

namespace DataServices.Json
{
    /// <summary>
    /// JPath exception.
    /// </summary>
    public class JPathException : Exception
    {

        /// <summary>
        /// Creates an instance of JPathException.
        /// </summary>
        public JPathException()
        {

        }

        /// <summary>
        /// Creates an instance of JPathException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public JPathException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates an instance of JPathException.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public JPathException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected JPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


    }
}
