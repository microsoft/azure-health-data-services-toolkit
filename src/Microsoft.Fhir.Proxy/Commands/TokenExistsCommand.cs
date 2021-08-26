using Microsoft.Health.Fhir.Proxy.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Commands
{
    public class TokenExistsCommand : IExceptionCommand
    {
        public TokenExistsCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        public void Execute()
        {
            if (token.Exists(jpath))
            {
                return;
            }

            throw new CommandException($"Fault TokenExists command with {jpath}.");
        }
    }
}
