namespace EasySave.Core.Models.BackupStrategies
{
    /// <summary>
    /// Factory class responsible for creating the appropriate backup strategy 
    /// based on the specified backup type.
    /// </summary>
    public static class BackupStrategyFactory
    {
        /// <summary>
        /// Returns the corresponding backup strategy based on the given backup type.
        /// </summary>
        /// <param name="type">The type of backup (Complete or Differential).</param>
        /// <returns>An instance of <see cref="IBackupStrategy"/> corresponding to the backup type.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid backup type is provided.</exception>
        public static IBackupStrategy GetStrategy(BackupType type)
        {
            return type switch
            {
                BackupType.Complete => new FullBackupStrategy(), // Full backup strategy
                BackupType.Differential => new DifferentialBackupStrategy(), // Differential backup strategy
                _ => throw new ArgumentException("Invalid backup type") // Throw exception for invalid backup type
            };
        }
    }
}
