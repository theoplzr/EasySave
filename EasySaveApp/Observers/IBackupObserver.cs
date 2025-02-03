namespace EasySaveApp.Observers
{
    public interface IBackupObserver
    {
        /// <summary>
        /// Method used to notify a change in the state of the backup.
        /// </summary>
        /// <param name="state">The updated status of the backup.</param>
        void Update(Models.BackupState state);
    }
}
