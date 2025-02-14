using Newtonsoft.Json;
using System;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents a completed backup job with detailed execution information.
    /// </summary>
    public class FinishedBackupJob
    {
        /// <summary>
        /// Unique identifier for the completed backup job.
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the backup job.
        /// </summary>
        [JsonProperty("BackupName")]
        public string Name { get; set; }

        /// <summary>
        /// The source directory where the original files were stored.
        /// </summary>
        [JsonProperty("SourceFilePath")]
        public string SourceDirectory { get; set; }

        /// <summary>
        /// The target directory where the backup files were saved.
        /// </summary>
        [JsonProperty("TargetFilePath")]
        public string TargetDirectory { get; set; }

        /// <summary>
        /// The total size of the backed-up file in bytes.
        /// </summary>
        [JsonProperty("FileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// The time taken to transfer the file in milliseconds.
        /// </summary>
        [JsonProperty("TransferTimeMs")]
        public int TransferTimeMs { get; set; }

        /// <summary>
        /// The time taken to encrypt the file in milliseconds.
        /// </summary>
        [JsonProperty("EncryptionTimeMs")]
        public int EncryptionTimeMs { get; set; }

        /// <summary>
        /// The status of the backup job (e.g., "Success", "Failed").
        /// </summary>
        [JsonProperty("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Indicates the backup priority level.
        /// </summary>
        [JsonProperty("Level")]
        public int Level { get; set; }

        /// <summary>
        /// The timestamp of when the backup was completed.
        /// </summary>
        [JsonProperty("Timestamp")]
        public DateTime CompletionTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the FinishedBackupJob class.
        /// </summary>
        /// <param name="name">The name of the backup job.</param>
        /// <param name="sourceDirectory">The source directory path.</param>
        /// <param name="targetDirectory">The target directory path.</param>
        /// <param name="fileSize">The size of the file in bytes.</param>
        /// <param name="transferTimeMs">The time taken for transfer in milliseconds.</param>
        /// <param name="encryptionTimeMs">The time taken for encryption in milliseconds.</param>
        /// <param name="status">The status of the backup job.</param>
        /// <param name="level">The backup level.</param>
        /// <param name="completionTime">The timestamp of completion.</param>
        public FinishedBackupJob(string name, string sourceDirectory, string targetDirectory, long fileSize, 
                                int transferTimeMs, int encryptionTimeMs, string status, int level, DateTime completionTime)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "Backup name cannot be null.");
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory), "Source directory cannot be null.");
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory), "Target directory cannot be null.");
            FileSize = fileSize;
            TransferTimeMs = transferTimeMs;
            EncryptionTimeMs = encryptionTimeMs;
            Status = status ?? "Unknown";
            Level = level;
            CompletionTime = completionTime;
        }

        /// <summary>
        /// Returns a string representation of the completed backup job.
        /// </summary>
        /// <returns>A string containing job name, status, and completion time.</returns>
        public override string ToString()
        {
            return $"[Completed Backup] Name: {Name}, Status: {Status}, Completed At: {CompletionTime}, Level: {Level}, Size: {FileSize} bytes, Transfer Time: {TransferTimeMs} ms, Encryption Time: {EncryptionTimeMs} ms, Source: {SourceDirectory}, Target: {TargetDirectory}";
        }
    }    
}
