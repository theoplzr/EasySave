using Avalonia.Threading;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.GUI.ViewModels;
using EasySave.GUI.Helpers; 
using System.Diagnostics;
using System.Linq;

namespace EasySave.GUI.Observers
{
    public class UiObserver : IBackupObserver
    {
        private readonly MainWindowViewModel _viewModel;

        public UiObserver(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Update(BackupState state)
        {
            if (string.IsNullOrEmpty(state.BackupName))
            {
                _viewModel.RealTimeStatus = "🚨 Erreur: Job introuvable.";
                return;
            }

            var localizedStatus = LanguageHelper.Instance.TranslateStatus(state.Status);

            // Trouver le job correspondant dans la liste
            var job = _viewModel.BackupJobs.FirstOrDefault(j => j.Name == state.BackupName);
            if (job != null)
            {
                job.Status = state.Status; // Mise à jour du statut dans le modèle
            }

            Dispatcher.UIThread.Post(() =>
            {
                // Afficher le statut "Démarrage" avant "Running"
                if (state.Status == "Starting")
                {
                    _viewModel.RealTimeStatus = $"🚀 Job \"{state.BackupName}\" en sauvegarde...";
                }
                else
                {
                    _viewModel.RealTimeStatus = $"⏳ Job \"{state.BackupName}\" => {state.ProgressPercentage}% ({localizedStatus})";
                }

                // Vérifier si TOUS les jobs sont terminés
                if (_viewModel.BackupJobs.All(j => j.Status == "Finished"))
                {
                    _viewModel.RealTimeStatus = "🎉 Toutes les sauvegardes sont terminées !";
                }
            });

            Debug.WriteLine($"[UI] Mise à jour de l'état : {state.Status} pour le job \"{state.BackupName}\"");
        }

    }
}
