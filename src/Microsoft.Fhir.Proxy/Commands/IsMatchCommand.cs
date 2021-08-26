using Microsoft.Health.Fhir.Proxy.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Commands
{
    public class IsMatchCommand<T> : IExceptionCommand
    {
        public IsMatchCommand(JToken token, string jpath, T? value)
        {
            this.token = token;
            this.jpath = jpath;
            this.value = value;
        }

        private readonly JToken token;
        private readonly string jpath;
        private readonly T? value;

        public void Execute()
        {
            if (token.IsMatch<T>(jpath, value))
            {
                return;
            }

            throw new CommandException($"Fault IsMatch command with {jpath} and {value}.");
        }
    }
}
