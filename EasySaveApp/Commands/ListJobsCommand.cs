namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour lister tous les jobs de sauvegarde configurés.
    /// </summary>
    public class ListJobsCommand : BackupCommand
    {
        public ListJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        public override void Execute()
        {
            _backupManager.ListBackupJobs();
        }
    }
}
