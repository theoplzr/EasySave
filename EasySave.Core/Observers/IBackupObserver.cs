namespace EasySave.Core.Observers
{
    /// <summary>
    /// Defines a mechanism for objects that observe backup jobs and get notified on state changes.
    /// </summary>
    public interface IBackupObserver
    {
        /// <summary>
        /// Notifies the observer about a change in the state of the backup.
        /// </summary>
        /// <param name="state">The updated status of the backup.</param>
        void Update(Models.BackupState state);
    }
}
