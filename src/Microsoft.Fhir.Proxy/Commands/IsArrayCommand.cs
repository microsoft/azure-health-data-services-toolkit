using Microsoft.Health.Fhir.Proxy.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Proxy.Commands
{
    public class IsArrayCommand : IExceptionCommand
    {
        public IsArrayCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        public void Execute()
        {
            if (token.IsArray(jpath))
            {
                return;
            }

            throw new CommandException($"Fault IsArray command with {jpath}.");
        }
    }
}
