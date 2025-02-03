using EasySaveApp.Models;
using Newtonsoft.Json;

namespace EasySaveApp.Observers
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
        /// Method called each time a backup state is updated.
        /// Updates the local dictionary and writes all states to the JSON file.
        /// </summary>
        /// <param name="state">The current state of a given backup job.</param>
        public void Update(BackupState state)
        {
            // Update the state in the dictionary
            _states[state.JobId] = state;

            // Write all states to the JSON file
            var allStates = _states.Values.ToList();
            File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(allStates, Formatting.Indented));
        }
    }
}
