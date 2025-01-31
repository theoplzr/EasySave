using Newtonsoft.Json;

namespace EasySaveApp.Models
{
    public class BackupState
    {
        public required string BackupName { get; set; }
        public DateTime LastActionTime { get; set; }
        public required string Status { get; set; } 
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public required string CurrentSourceFile { get; set; }
        public required string CurrentTargetFile { get; set; }
        public Guid JobId { get; set; }

        public static void SaveState(List<BackupState> states, string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(states, Formatting.Indented));
        }
    }
}
