using EasySaveApp.Commands;
using EasySaveApp.Models;
using EasySaveApp.Observers;
using EasySaveApp.Repositories;

namespace EasySaveApp.Facade
{
    /// <summary>
    /// Fournit une interface simplifiée (façade) pour effectuer
    /// les opérations principales d'EasySave sans exposer les détails internes.
    /// </summary>
    public class EasySaveFacade
    {
        private readonly BackupManager _backupManager;

        /// <summary>
        /// Construit la façade EasySave en injectant les dépendances nécessaires :
        /// - Un repository (pour la persistance des BackupJobs)
        /// - Un chemin de logs (pour le logger)
        /// - Un observer (facultatif) pour le fichier d'état.
        /// </summary>
        /// <param name="jobRepository">Le repository pour gérer la persistance des BackupJobs.</param>
        /// <param name="logDirectory">Le répertoire pour enregistrer les logs.</param>
        /// <param name="stateObserver">Observer pour la mise à jour de l'état (fichier JSON), peut être null.</param>
        public EasySaveFacade(IBackupJobRepository jobRepository, string logDirectory, IBackupObserver? stateObserver = null)
        {
            // Création du BackupManager avec le repository et le répertoire de logs
            _backupManager = new BackupManager(jobRepository, logDirectory);

            // Si un observer d'état est fourni, on l'ajoute
            if (stateObserver != null)
            {
                _backupManager.AddObserver(stateObserver);
            }
        }

        /// <summary>
        /// Ajoute un nouveau job de sauvegarde.
        /// </summary>
        /// <param name="job">Le BackupJob à ajouter.</param>
        public void AddJob(BackupJob job)
        {
            var cmd = new AddJobCommand(_backupManager, job);
            cmd.Execute();
        }

        /// <summary>
        /// Supprime un job existant, identifié par son index (zéro-based).
        /// </summary>
        /// <param name="index">L'index du job à supprimer.</param>
        public void RemoveJob(int index)
        {
            var cmd = new RemoveJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Met à jour un job existant, en fournissant éventuellement
        /// de nouvelles valeurs pour le nom, la source, la cible et le type.
        /// Les paramètres null ou vides ne seront pas mis à jour.
        /// </summary>
        /// <param name="index">Index du job (zéro-based).</param>
        /// <param name="newName">Nouveau nom (ou null pour garder l'ancien).</param>
        /// <param name="newSource">Nouveau répertoire source (ou null).</param>
        /// <param name="newTarget">Nouveau répertoire cible (ou null).</param>
        /// <param name="newType">Nouveau type de sauvegarde (ou null).</param>
        public void UpdateJob(int index, string? newName, string? newSource, string? newTarget, BackupType? newType)
        {
            var cmd = new UpdateJobCommand(_backupManager, index, newName, newSource, newTarget, newType);
            cmd.Execute();
        }

        /// <summary>
        /// Exécute tous les jobs de sauvegarde.
        /// </summary>
        public void ExecuteAllJobs()
        {
            var cmd = new ExecuteAllJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Exécute un job de sauvegarde précis, identifié par son index (zéro-based).
        /// </summary>
        /// <param name="index">Index du job à exécuter.</param>
        public void ExecuteJobByIndex(int index)
        {
            var cmd = new ExecuteJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Affiche la liste des jobs existants (nom, source, cible, type).
        /// </summary>
        public void ListJobs()
        {
            var cmd = new ListJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Retourne le nombre de jobs configurés.
        /// </summary>
        /// <returns>Le nombre de BackupJobs configurés.</returns>
        public int GetJobCount()
        {
            return _backupManager.GetBackupJobCount();
        }
    }
}
