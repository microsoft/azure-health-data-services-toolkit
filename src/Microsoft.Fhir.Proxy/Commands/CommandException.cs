using System;
using System.Runtime.Serialization;

namespace Microsoft.Fhir.Proxy.Commands
{
    public class CommandException : Exception
    {
        public CommandException()
        {

        }

        public CommandException(string message)
            : base(message)
        {

        }


        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected CommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
