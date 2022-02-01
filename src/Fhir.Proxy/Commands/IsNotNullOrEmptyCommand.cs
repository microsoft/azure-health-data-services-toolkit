using Fhir.Proxy.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Commands
{
    /// <summary>
    /// Command the verifies a JToken with Json path has to non-null or non-empty value.
    /// </summary>
    public class IsNotNullOrEmptyCommand : IExceptionCommand
    {
        /// <summary>
        /// Creates an instance of IsNotNullOrEmptyCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path to test for not null or empty in JToken.</param>
        public IsNotNullOrEmptyCommand(JToken token, string jpath)
        {
            this.token = token;
            this.jpath = jpath;
        }

        private readonly JToken token;
        private readonly string jpath;

        /// <summary>
        /// Executes without exception if Json path for JToken is non-null or non-empty.
        /// </summary>
        public void Execute()
        {
            if (!token.IsNullOrEmpty(jpath))
            {
                return;
            }

            throw new CommandException($"Fault IsNotNullOrEmpty command with {jpath}.");
        }
    }
}
