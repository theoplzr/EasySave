namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour lister tous les jobs de sauvegarde configurés.
    /// </summary>
    public class ListJobsCommand : BackupCommand
    {
        // Constructor to initialize the ListJobsCommand with the backup manager
        public ListJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        // Method to execute the command to list all backup jobs
        public override void Execute()
        {
            _backupManager.ListBackupJobs();
        }
    }
}