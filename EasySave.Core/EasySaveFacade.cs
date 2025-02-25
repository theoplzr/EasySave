using EasySave.Core.Commands;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using Microsoft.Extensions.Configuration;

namespace EasySave.Core.Facade
{
    /// <summary>
    /// Fournit une interface simplifiée pour effectuer les opérations principales d'EasySave.
    /// </summary>
    public class EasySaveFacade
    {
        private readonly BackupManager _backupManager;

        public EasySaveFacade(IBackupJobRepository jobRepository, string logDirectory, IBackupObserver? stateObserver, IConfiguration configuration)
        {
            _backupManager = new BackupManager(jobRepository, logDirectory, configuration);
            if (stateObserver != null)
            {
                _backupManager.AddObserver(stateObserver);
            }
        }

        public void AddJob(BackupJob job)
        {
            var cmd = new AddJobCommand(_backupManager, job);
            cmd.Execute();
        }

        public void RemoveJob(int index)
        {
            var cmd = new RemoveJobCommand(_backupManager, index);
            cmd.Execute();
        }

        public int GetJobIndexById(Guid id)
        {
            return _backupManager.GetAllJobs().FindIndex(job => job.Id == id);
        }

        public void UpdateJob(int index, string? newName, string? newSource, string? newTarget, BackupType? newType)
        {
            var cmd = new UpdateJobCommand(_backupManager, index, newName, newSource, newTarget, newType);
            cmd.Execute();
        }

        public void ExecuteAllJobs()
        {
            var cmd = new ExecuteAllJobsCommand(_backupManager);
            cmd.Execute();
        }

        public void ExecuteJobByIndex(int index)
        {
            var cmd = new ExecuteJobCommand(_backupManager, index);
            cmd.Execute();
        }

        public void ListJobs()
        {
            var cmd = new ListJobsCommand(_backupManager);
            cmd.Execute();
        }

        public int GetJobCount()
        {
            return _backupManager.GetBackupJobCount();
        }

        public List<BackupJob> ListBackupJobs()
        {
            return _backupManager.GetAllJobs();
        }

        public void AddObserver(IBackupObserver observer)
        {
            _backupManager.AddObserver(observer);
        }

        /// <summary>
        /// Retourne le statut actuel du système de sauvegarde.
        /// </summary>
        public string GetStatus()
        {
            return _backupManager.GetStatus();
        }

        public void PauseJob(Guid jobId)
        {
            _backupManager.PauseJob(jobId);
        }

        public void ResumeJob(Guid jobId)
        {
            _backupManager.ResumeJob(jobId);
        }

        public void StopJob(Guid jobId)
        {
            _backupManager.StopJob(jobId);
        }
    }
}
