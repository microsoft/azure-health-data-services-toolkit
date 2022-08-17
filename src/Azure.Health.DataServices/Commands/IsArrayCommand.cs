using Azure.Health.DataServices.Json;
using Newtonsoft.Json.Linq;

namespace Azure.Health.DataServices.Commands
{
    /// <summary>
    /// Command the verifies a JToken with Json path terminates at a JArray.
    /// </summary>
    public class IsArrayCommand : ICommandException
    {
        /// <summary>
        /// Creates an instance of IsArrayCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path in JToken to excepted JArray.</param>
        public IsArrayCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        /// <summary>
        /// Executes without exception if Json path for JToken is JArray.
        /// </summary>
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
