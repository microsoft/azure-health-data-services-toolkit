using System;
using System.Runtime.Serialization;

namespace Microsoft.Health.Fhir.Proxy.Json
{
    public class JPathException : Exception
    {

        public JPathException()
        {

        }

        public JPathException(string message)
            : base(message)
        {

        }


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
