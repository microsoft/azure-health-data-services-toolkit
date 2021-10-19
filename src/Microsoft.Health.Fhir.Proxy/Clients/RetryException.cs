using System;
using System.Runtime.Serialization;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    public class RetryException : Exception
    {
        public RetryException()
        {
        }

        public RetryException(string message)
            : base(message)
        {

        }

        public RetryException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected RetryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
