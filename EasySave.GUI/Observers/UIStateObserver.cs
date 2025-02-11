using EasySave.Core.Observers;
using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace EasySave.GUI.Observers
{
    public class UIStateObserver : IBackupObserver
    {
        // Référence vers la collection d'états dans la GUI
        private readonly ObservableCollection<BackupState> _uiStates;

        public UIStateObserver(ObservableCollection<BackupState> uiStates)
        {
            _uiStates = uiStates;
        }

        public void Update(BackupState state)
        {
            // On met à jour ou on remplace l'état existant
            // On recherche s'il existe déjà un BackupState avec le même JobId
            var existing = _uiStates.FirstOrDefault(s => s.JobId == state.JobId);
            if (existing != null)
            {
                // Mettre à jour chaque champ
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
                // Ajouter un nouveau BackupState
                _uiStates.Add(state);
            }
        }
    }
}
