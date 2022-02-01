using Fhir.Proxy.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Commands
{
    /// <summary>
    /// Command the verifies a JToken exists for Json path in a parent JToken.
    /// </summary>
    public class TokenExistsCommand : IExceptionCommand
    {
        /// <summary>
        /// Creates an instance of TokenExistsCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path to test for exists.</param>
        public TokenExistsCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        /// <summary>
        /// Executes without exception if JToken exists in the Json path.
        /// </summary>
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
