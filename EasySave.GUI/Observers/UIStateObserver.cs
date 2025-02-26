using EasySave.Core.Models;
using EasySave.Core.Observers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.GUI.Observers
{
    /// <summary>
    /// An observer that maintains an in-memory and on-disk representation
    /// of backup states, updating a shared <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public class UIStateObserver : IBackupObserver
    {
        private readonly ObservableCollection<BackupState> _uiStates;

        // Default path for saving backup states to disk
        private readonly string _stateFilePath = "state.json";

        /// <summary>
        /// Initializes a new instance of <see cref="UIStateObserver"/>.
        /// </summary>
        /// <param name="uiStates">The UI collection where backup states are stored.</param>
        public UIStateObserver(ObservableCollection<BackupState> uiStates)
        {
            _uiStates = uiStates;
        }

        /// <summary>
        /// Receives and stores a backup job state, updating the local collection and state file.
        /// </summary>
        /// <param name="state">The current backup job state.</param>
        public void Update(BackupState state)
        {
            // Check if this job ID already exists in the collection
            var existing = _uiStates.FirstOrDefault(s => s.JobId == state.JobId);
            if (existing != null)
            {
                // Update the existing record
                existing.BackupName = state.BackupName;
                existing.LastActionTime = state.LastActionTime;
                existing.Status = state.Status;
                existing.TotalFiles = state.TotalFiles;
                existing.TotalSize = state.TotalSize;
                existing.RemainingFiles = state.RemainingFiles;
                existing.RemainingSize = state.RemainingSize;
                existing.CurrentSourceFile = state.CurrentSourceFile;
                existing.CurrentTargetFile = state.CurrentTargetFile;
            }
            else
            {
                // Add a new entry
                _uiStates.Add(state);
            }

            SaveStateToFile();
        }

        /// <summary>
        /// Saves the current collection of backup states to a JSON file.
        /// </summary>
        private void SaveStateToFile()
        {
            try
            {
                var serialized = JsonSerializer.Serialize(_uiStates, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_stateFilePath, serialized);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error writing to '{_stateFilePath}': {ex.Message}");
            }
        }
    }
}
