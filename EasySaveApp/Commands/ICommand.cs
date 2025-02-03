namespace EasySaveApp.Commands
{
    /// <summary>
    /// Defines the structure of a command in the application.
    /// Each command must implement the <see cref="Execute"/> method.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Execute();
    }
}
