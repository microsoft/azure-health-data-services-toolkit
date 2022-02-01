namespace Fhir.Proxy.Commands
{
    /// <summary>
    /// Interface for commands.
    /// </summary>
    public interface IExceptionCommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Execute();
    }
}
