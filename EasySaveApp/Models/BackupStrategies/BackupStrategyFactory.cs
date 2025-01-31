namespace EasySaveApp.Models.BackupStrategies
{
    public static class BackupStrategyFactory
    {
        public static IBackupStrategy GetStrategy(BackupType type)
        {
            return type switch
            {
                BackupType.Complete => new FullBackupStrategy(),
                BackupType.Differential => new DifferentialBackupStrategy(),
                _ => throw new ArgumentException("Invalid backup type")
            };
        }
    }
}
