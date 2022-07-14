namespace DataServices.Commands
{
    /// <summary>
    /// Interface for commands.
    /// </summary>
    public interface ICommandException
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Execute();
    }
}
