namespace EasySave.Core.Models
{
    /// <summary>
    /// Describes the possible states of a backup job.
    /// </summary>
    public enum JobState
    {
        /// <summary>
        /// The backup job is currently running.
        /// </summary>
        Running,

        /// <summary>
        /// The backup job is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The backup job has been stopped.
        /// </summary>
        Stopped
    }
}
