using EasySaveApp.Models;
using Newtonsoft.Json;

namespace EasySaveApp.Observers
{
    /// <summary>
    /// Observateur chargé d'écrire l'état actuel des sauvegardes dans un fichier JSON unique.
    /// </summary>
    public class FileStateObserver : IBackupObserver
    {
        private readonly string _stateFilePath;
        private readonly Dictionary<Guid, BackupState> _states;

        public FileStateObserver(string stateFilePath)
        {
            _stateFilePath = stateFilePath;
            _states = new Dictionary<Guid, BackupState>();
        }

        /// <summary>
        /// Méthode appelée à chaque mise à jour de l'état.
        /// Met à jour le dictionnaire local et écrit l'ensemble des états dans le fichier.
        /// </summary>
        /// <param name="state">L'état actuel pour un job donné.</param>
        public void Update(BackupState state)
        {
            // Mettre à jour l'état dans le dictionnaire
            _states[state.JobId] = state;

            // Écrire l'ensemble des états dans le fichier JSON
            var allStates = _states.Values.ToList();
            File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(allStates, Formatting.Indented));
        }
    }
}
