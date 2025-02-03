// classe qui permet de créer une stratégie de sauvegarde en fonction du type de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public static class BackupStrategyFactory
    {
        // Method to get the appropriate backup strategy based on the backup type
        public static IBackupStrategy GetStrategy(BackupType type)
        {
            // Return the corresponding backup strategy based on the backup type
            return type switch
            {
                BackupType.Complete => new FullBackupStrategy(), // Full backup strategy
                BackupType.Differential => new DifferentialBackupStrategy(), // Differential backup strategy
                _ => throw new ArgumentException("Invalid backup type") // Throw exception for invalid backup type
            };
        }
    }
}
