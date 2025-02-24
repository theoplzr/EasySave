using System;
using System.IO;
using System.Text.Json;
using EasySave.Core.Observers;
using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace EasySave.GUI.Observers
{
    public class UIStateObserver : IBackupObserver
    {
        private readonly ObservableCollection<BackupState> _uiStates;
        private readonly string _stateFilePath = "state.json";

        public UIStateObserver(ObservableCollection<BackupState> uiStates)
        {
            _uiStates = uiStates;
        }

        public void Update(BackupState state)
        {
            var existing = _uiStates.FirstOrDefault(s => s.JobId == state.JobId);
            if (existing != null)
            {
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
                _uiStates.Add(state);
            }

            SaveStateToFile();
        }

        private void SaveStateToFile()
        {
            try
            {
                File.WriteAllText(_stateFilePath, JsonSerializer.Serialize(_uiStates, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"❌ Erreur d'écriture de state.json : {ex.Message}");
            }
        }
    }
}
