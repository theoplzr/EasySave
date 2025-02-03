//classe pour gérer l'état de la sauvegarde

using Newtonsoft.Json;

namespace EasySaveApp.Models
{
    public class BackupState
    {
        // Name of the backup
        public required string BackupName { get; set; }
        
        // Time of the last action performed
        public DateTime LastActionTime { get; set; }
        
        // Current status of the backup
        public required string Status { get; set; } 
        
        // Total number of files to be backed up
        public int TotalFiles { get; set; }
        
        // Total size of files to be backed up
        public long TotalSize { get; set; }
        
        // Number of files remaining to be backed up
        public int RemainingFiles { get; set; }
        
        // Size of files remaining to be backed up
        public long RemainingSize { get; set; }
        
        // Current source file being backed up
        public required string CurrentSourceFile { get; set; }
        
        // Current target file being backed up
        public required string CurrentTargetFile { get; set; }
        
        // Unique identifier for the backup job
        public Guid JobId { get; set; }

        // Method to save the state of backups to a file
        public static void SaveState(List<BackupState> states, string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(states, Formatting.Indented));
        }
    }
}
