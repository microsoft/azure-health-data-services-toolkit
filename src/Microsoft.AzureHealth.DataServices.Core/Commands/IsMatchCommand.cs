using Microsoft.AzureHealth.DataServices.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Commands
{
    /// <summary>
    /// Command the verifies a JToken property value from Json path matches the supplied value.
    /// </summary>
    /// <typeparam name="T"> is the type of the value to test for match.</typeparam>
    public class IsMatchCommand<T> : ICommandException
    {
        private readonly JToken token;
        private readonly string jpath;
        private readonly T value;

        /// <summary>
        /// Creates an instance of IsMatchCommand.
        /// </summary>
        /// <param name="token">JToken to test.</param>
        /// <param name="jpath">Json path in JToken to leads to property to test value.</param>
        /// <param name="value">Value to test for match.</param>
        public IsMatchCommand(JToken token, string jpath, T value)
        {
            this.token = token;
            this.jpath = jpath;
            this.value = value;
        }

        /// <summary>
        /// Executes without exception if Json path for JToken has a property value that matches supplied value.
        /// </summary>
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
