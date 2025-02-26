using EasySave.Core.Commands;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace EasySave.Core.Facade
{
    /// <summary>
    /// Provides a simplified interface (facade) for performing main EasySave operations.
    /// </summary>
    public class EasySaveFacade
    {
        private readonly BackupManager _backupManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasySaveFacade"/> class.
        /// </summary>
        /// <param name="jobRepository">Repository for persisting backup jobs.</param>
        /// <param name="logDirectory">Directory where logs are stored.</param>
        /// <param name="stateObserver">Optional observer to receive backup state updates.</param>
        /// <param name="configuration">Configuration object for custom settings.</param>
        public EasySaveFacade(
            IBackupJobRepository jobRepository, 
            string logDirectory, 
            IBackupObserver? stateObserver, 
            IConfiguration configuration
        )
        {
            _backupManager = new BackupManager(jobRepository, logDirectory, configuration);
            if (stateObserver != null)
            {
                _backupManager.AddObserver(stateObserver);
            }
        }

        /// <summary>
        /// Adds a new backup job via a command object.
        /// </summary>
        /// <param name="job">The new <see cref="BackupJob"/> to add.</param>
        public void AddJob(BackupJob job)
        {
            var cmd = new AddJobCommand(_backupManager, job);
            cmd.Execute();
        }

        /// <summary>
        /// Removes a backup job by index.
        /// </summary>
        /// <param name="index">Zero-based index of the job to remove.</param>
        public void RemoveJob(int index)
        {
            var cmd = new RemoveJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Retrieves a job's index by its unique identifier (GUID).
        /// </summary>
        /// <param name="id">The job's <see cref="Guid"/>.</param>
        /// <returns>An integer representing the job's zero-based index, or -1 if not found.</returns>
        public int GetJobIndexById(Guid id)
        {
            return _backupManager.GetAllJobs().FindIndex(job => job.Id == id);
        }

        /// <summary>
        /// Updates properties of a backup job by its zero-based index.
        /// </summary>
        /// <param name="index">The job index.</param>
        /// <param name="newName">Optional new name.</param>
        /// <param name="newSource">Optional new source directory.</param>
        /// <param name="newTarget">Optional new target directory.</param>
        /// <param name="newType">Optional new <see cref="BackupType"/>.</param>
        public void UpdateJob(int index, string? newName, string? newSource, string? newTarget, BackupType? newType)
        {
            var cmd = new UpdateJobCommand(_backupManager, index, newName, newSource, newTarget, newType);
            cmd.Execute();
        }

        /// <summary>
        /// Initiates the execution of all backup jobs asynchronously.
        /// </summary>
        public void ExecuteAllJobs()
        {
            var cmd = new ExecuteAllJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Executes a single backup job by its zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the job.</param>
        public void ExecuteJobByIndex(int index)
        {
            var cmd = new ExecuteJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Prints a list of configured backup jobs to the console.
        /// </summary>
        public void ListJobs()
        {
            var cmd = new ListJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Retrieves the number of currently configured backup jobs.
        /// </summary>
        /// <returns>An integer representing the count of jobs.</returns>
        public int GetJobCount()
        {
            return _backupManager.GetBackupJobCount();
        }

        /// <summary>
        /// Retrieves a list of all configured backup jobs.
        /// </summary>
        /// <returns>A list of <see cref="BackupJob"/> objects.</returns>
        public List<BackupJob> ListBackupJobs()
        {
            return _backupManager.GetAllJobs();
        }

        /// <summary>
        /// Adds an <see cref="IBackupObserver"/> instance to track backup state changes.
        /// </summary>
        /// <param name="observer">The observer to add.</param>
        public void AddObserver(IBackupObserver observer)
        {
            _backupManager.AddObserver(observer);
        }

        /// <summary>
        /// Gets the current overall status of the backup system.
        /// </summary>
        /// <returns>A string containing the status.</returns>
        public string GetStatus()
        {
            return _backupManager.GetStatus();
        }

        /// <summary>
        /// Pauses the backup job with the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        public void PauseJob(Guid jobId)
        {
            _backupManager.PauseJob(jobId);
        }

        /// <summary>
        /// Resumes a paused backup job with the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        public void ResumeJob(Guid jobId)
        {
            _backupManager.ResumeJob(jobId);
        }

        /// <summary>
        /// Stops the backup job with the specified <see cref="Guid"/>, canceling any ongoing tasks.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        public void StopJob(Guid jobId)
        {
            _backupManager.StopJob(jobId);
        }
    }
}
