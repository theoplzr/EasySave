using Avalonia.Threading;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.GUI.Helpers;
using EasySave.GUI.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace EasySave.GUI.Observers
{
    /// <summary>
    /// An observer responsible for updating the UI based on backup state changes.
    /// Implements <see cref="IBackupObserver"/>.
    /// </summary>
    public class UiObserver : IBackupObserver
    {
        private readonly MainWindowViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of <see cref="UiObserver"/>.
        /// </summary>
        /// <param name="viewModel">The main window ViewModel containing UI state and properties.</param>
        public UiObserver(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Receives state updates for a backup job and updates the UI accordingly.
        /// </summary>
        /// <param name="state">The current state of a backup job.</param>
        public void Update(BackupState state)
        {
            // If the BackupName is missing, the job is not found
            if (string.IsNullOrEmpty(state.BackupName))
            {
                _viewModel.RealTimeStatus = "🚨 Error: Job not found.";
                return;
            }

            // Localize the status message (e.g., "Running", "Paused", etc.)
            var localizedStatus = LanguageHelper.Instance.TranslateStatus(state.Status);

            // Find the corresponding job in the list
            var job = _viewModel.BackupJobs.FirstOrDefault(j => j.Name == state.BackupName);
            if (job != null)
            {
                // Update the job's status in the model
                job.Status = state.Status;
            }

            // Post an update to the Avalonia UI thread
            Dispatcher.UIThread.Post(() =>
            {
                if (state.Status == "Starting")
                {
                    _viewModel.RealTimeStatus = $"🚀 Job \"{state.BackupName}\" is starting its backup...";
                }
                else
                {
                    _viewModel.RealTimeStatus =
                        $"⏳ Job \"{state.BackupName}\" => {state.ProgressPercentage}% ({localizedStatus})";
                }

                // Check if ALL jobs have finished
                if (_viewModel.BackupJobs.All(j => j.Status == "Finished"))
                {
                    _viewModel.RealTimeStatus = "🎉 All backups are completed!";
                }
            });

            Debug.WriteLine($"[UI] State updated: {state.Status} for job \"{state.BackupName}\"");
        }
    }
}
