using Microsoft.AzureHealth.DataServices.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Commands
{
    /// <summary>
    /// Command the verifies a JToken with Json path is null or empty value.
    /// </summary>
    public class IsNullOrEmptyCommand : ICommandException
    {
        /// <summary>
        /// Creates an instance of IsNotNullOrEmptyCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path to test for null or empty in JToken.</param>
        public IsNullOrEmptyCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        /// <summary>
        /// Executes without exception if Json path for JToken is null or empty.
        /// </summary>
        public void Execute()
        {
            if (token.IsNullOrEmpty(jpath))
            {
                return;
            }

            throw new CommandException($"Fault IsNullOrEmpty command with {jpath}.");
        }
    }
}
