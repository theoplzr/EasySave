using EasySave.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.Core.Observers
{
    /// <summary>
    /// Observer responsible for writing the current state of backup jobs to a single JSON file.
    /// Implements <see cref="IBackupObserver"/> to track backup progress.
    /// </summary>
    public class FileStateObserver : IBackupObserver
    {
        /// <summary>
        /// The file path where backup states are stored.
        /// </summary>
        private readonly string _stateFilePath;

        /// <summary>
        /// Dictionary storing the state of each backup job, indexed by its unique identifier.
        /// </summary>
        private readonly Dictionary<Guid, BackupState> _states;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStateObserver"/> class.
        /// </summary>
        /// <param name="stateFilePath">The path of the JSON file used for state storage.</param>
        public FileStateObserver(string stateFilePath)
        {
            _stateFilePath = stateFilePath;
            _states = new Dictionary<Guid, BackupState>();
        }

        /// <summary>
        /// Called each time a backup state is updated.
        /// Updates the local dictionary and writes all states to the JSON file.
        /// </summary>
        /// <param name="state">The current state of a given backup job.</param>
        public void Update(BackupState state)
        {
            // Update or add the state in the dictionary
            _states[state.JobId] = state;

            // Retrieve all states from the dictionary
            var allStates = _states.Values.ToList();

            // Build the data (including progress) to be written to the JSON file
            var formattedStates = allStates.Select(s => new
            {
                s.BackupName,
                s.LastActionTime,
                s.Status,
                s.TotalFiles,
                s.TotalSize,
                s.RemainingFiles,
                s.RemainingSize,
                Progress = s.ProgressPercentage, 
                s.CurrentSourceFile,
                s.CurrentTargetFile,
                s.JobId
            }).ToList();

            // Serialize and write all states to the specified JSON file
            File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(formattedStates, Formatting.Indented));
        }
    }
}
