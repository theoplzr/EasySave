using Newtonsoft.Json;
using System;
using System.IO;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents the state of a backup job at a given moment.
    /// </summary>
    public class BackupState
    {
        /// <summary>
        /// Name of the backup job.
        /// </summary>
        public required string BackupName { get; set; }
        
        /// <summary>
        /// Timestamp of the last action performed during the backup process.
        /// </summary>
        public DateTime LastActionTime { get; set; }
        
        /// <summary>
        /// Current status of the backup job (e.g., "Active", "Completed", "Error").
        /// </summary>
        public required string Status { get; set; } 
        
        /// <summary>
        /// Total number of files to be backed up.
        /// </summary>
        public int TotalFiles { get; set; }
        
        /// <summary>
        /// Total size (in bytes) of all files to be backed up.
        /// </summary>
        public long TotalSize { get; set; }
        
        /// <summary>
        /// Number of files remaining to be backed up.
        /// </summary>
        public int RemainingFiles { get; set; }
        
        /// <summary>
        /// Size (in bytes) of files remaining to be backed up.
        /// </summary>
        public long RemainingSize { get; set; }
        
        /// <summary>
        /// Path of the current source file being backed up.
        /// </summary>
        public required string CurrentSourceFile { get; set; }
        
        /// <summary>
        /// Path of the current target file being backed up.
        /// </summary>
        public required string CurrentTargetFile { get; set; }
        
        /// <summary>
        /// Unique identifier for the backup job.
        /// </summary>
        public Guid JobId { get; set; }
        
        /// <summary>
        /// Progress of the backup job in percentage (0 to 100).
        /// This property is set explicitly (e.g. via NotifyProgress).
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Saves the current state of all backup jobs to a JSON file.
        /// </summary>
        /// <param name="states">List of <see cref="BackupState"/> instances to be saved.</param>
        /// <param name="filePath">The path of the JSON file where the state is stored.</param>
        public static void SaveState(List<BackupState> states, string filePath)
        {
            try
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(states, Formatting.Indented));
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error saving backup state: {ex.Message}");
            }
        }
    }
}
