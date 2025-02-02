namespace EasySaveApp.Observers
{
    public interface IBackupObserver
    {
        /// <summary>
        /// Méthode appelée pour notifier un changement d’état du backup.
        /// </summary>
        /// <param name="state">L’état mis à jour du backup.</param>
        void Update(Models.BackupState state);
    }
}
