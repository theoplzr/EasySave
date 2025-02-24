using EasySave.Core.Models;
using EasySaveLogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace EasySave.Core.Template
{
    /// <summary>
    /// Classe abstraite définissant le modèle des algorithmes de sauvegarde.
    /// Implémente le pattern Template Method pour structurer le processus de sauvegarde.
    /// </summary>
    public abstract class AbstractBackupAlgorithm
    {
        /// <summary>
        /// Instance du logger pour enregistrer les actions de sauvegarde.
        /// </summary>
        protected readonly Logger _logger;

        /// <summary>
        /// Callback pour notifier les observateurs des changements d'état de la sauvegarde.
        /// </summary>
        private readonly Action<BackupState>? _notifyObserver;

        /// <summary>
        /// Action pour sauvegarder les modifications de l'état de la sauvegarde.
        /// </summary>
        private readonly Action? _saveChanges;

        private string _status;
        private readonly string _businessSoftwareName;

        /// <summary>
        /// Constructeur de l'algorithme de sauvegarde.
        /// </summary>
        /// <param name="logger">Instance du logger pour enregistrer les actions.</param>
        /// <param name="notifyObserver">Action pour notifier les observateurs des changements d'état.</param>
        /// <param name="saveChanges">Action pour persister les modifications d'état.</param>
        protected AbstractBackupAlgorithm(
            Logger logger,
            Action<BackupState>? notifyObserver,
            Action? saveChanges,
            string businessSoftwareName
        )
        {
            _logger = logger;
            _notifyObserver = notifyObserver;
            _saveChanges = saveChanges;
            _businessSoftwareName = businessSoftwareName;
            _status = "waiting";
        }

        /// <summary>
        /// Exécute le processus de sauvegarde en suivant la logique définie.
        /// </summary>
        /// <param name="job">Le job de sauvegarde à exécuter.</param>
        public void Execute(BackupJob job)
        {
            Prepare(job); // Préparer le job de sauvegarde
            var files = GatherFiles(job); // Récupérer les fichiers à sauvegarder

            // Variables de suivi pour la progression
            int filesProcessed = 0;
            long bytesProcessed = 0;
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            int totalFiles = files.Count();

            // Traiter chaque fichier
            foreach (var file in files)
            {
                while (IsBusinessSoftwareRunning())
                {
                    _status = "paused";
                    Thread.Sleep(2000);
                }

                _status = "running";
                if (ShouldCopyFile(file, job))
                {
                    CopyFile(job, file, ref filesProcessed, ref bytesProcessed, totalFiles, totalSize);
                }
            }

            FinalizeBackup(job); // Finaliser le processus de sauvegarde

            // Sauvegarder les modifications si nécessaire
            _saveChanges?.Invoke();
        }

        /// <summary>
        /// Prépare l'environnement pour le job de sauvegarde.
        /// </summary>
        /// <param name="job">Le job de sauvegarde courant.</param>
        protected virtual void Prepare(BackupJob job)
        {
            _status = "preparation";
            Console.WriteLine($"[Template] Démarrage de la sauvegarde {job.Name} ...");

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine($"Le dossier source n'existe pas : {job.SourceDirectory}");
            }

            if (!Directory.Exists(job.TargetDirectory))
            {
                if (!string.IsNullOrEmpty(job.TargetDirectory))
                {
                    Directory.CreateDirectory(job.TargetDirectory);
                }
            }
        }

        /// <summary>
        /// Récupère la liste des fichiers à sauvegarder depuis le dossier source.
        /// </summary>
        /// <param name="job">Le job de sauvegarde.</param>
        /// <returns>Une collection de chemins de fichiers à sauvegarder.</returns>
        protected virtual IEnumerable<string> GatherFiles(BackupJob job)
        {
            return Directory.GetFiles(job.SourceDirectory, "*.*", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Détermine si un fichier doit être copié en fonction de la configuration du job.
        /// </summary>
        /// <param name="filePath">Le chemin du fichier.</param>
        /// <param name="job">Le job de sauvegarde.</param>
        /// <returns>Vrai si le fichier doit être copié, sinon faux.</returns>
        protected abstract bool ShouldCopyFile(string filePath, BackupJob job);

        /// <summary>
        /// Copie un fichier du dossier source vers le dossier cible.
        /// </summary>
        /// <param name="job">Le job de sauvegarde.</param>
        /// <param name="filePath">Le chemin du fichier à copier.</param>
        /// <param name="filesProcessed">Référence au nombre de fichiers traités.</param>
        /// <param name="bytesProcessed">Référence à la taille totale des fichiers traités.</param>
        /// <param name="totalFiles">Nombre total de fichiers à traiter.</param>
        /// <param name="totalSize">Taille totale des fichiers à sauvegarder.</param>
        protected abstract void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        );

        /// <summary>
        /// Effectue les actions finales après la sauvegarde.
        /// </summary>
        /// <param name="job">Le job de sauvegarde terminé.</param>
        protected virtual void FinalizeBackup(BackupJob job)
        {
            _status = "finished";
            Console.WriteLine($"[Template] Sauvegarde '{job.Name}' terminée.");
        }

        /// <summary>
        /// Notifie les observateurs de l'état courant de la sauvegarde.
        /// </summary>
        /// <param name="state">L'état courant de la sauvegarde.</param>
        protected void Notify(BackupState state)
        {
            _notifyObserver?.Invoke(state);
        }

        /// <summary>
        /// Enregistre une action dans le système de log.
        /// </summary>
        /// <param name="entry">Le log à enregistrer.</param>
        protected void LogAction(LogEntry entry)
        {
            _logger.LogAction(entry);
        }

        public string GetStatus()
        {
            return _status;
        }

        public bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcessesByName(_businessSoftwareName).Any();
        }
    }
}
