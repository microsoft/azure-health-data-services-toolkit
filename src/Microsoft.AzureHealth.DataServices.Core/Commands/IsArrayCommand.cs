using Microsoft.AzureHealth.DataServices.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Commands
{
    /// <summary>
    /// Command the verifies a JToken with Json path terminates at a JArray.
    /// </summary>
    public class IsArrayCommand : ICommandException
    {
        private readonly JToken _token;
        private readonly string _jpath;

        /// <summary>
        /// Creates an instance of IsArrayCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path in JToken to excepted JArray.</param>
        public IsArrayCommand(JToken token, string jpath)
        {
            _token = token;
            _jpath = jpath;
        }

        /// <summary>
        /// Executes without exception if Json path for JToken is JArray.
        /// </summary>
        public void Execute()
        {
            if (_token.IsArray(_jpath))
            {
                return;
            }

            throw new CommandException($"Fault IsArray command with {_jpath}.");
        }
    }
}
