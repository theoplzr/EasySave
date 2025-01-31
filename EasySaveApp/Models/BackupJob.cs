using EasySaveApp.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySaveApp.Models
{
    /// <summary>
    /// Représente un travail de sauvegarde.
    /// Un travail de sauvegarde est défini par un nom, un répertoire source, un répertoire cible et le type de sauvegarde (complète ou différentielle).
    /// </summary>
    public class BackupJob
    {
        // Propriétés du travail de sauvegarde
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }               // Nom du travail de sauvegarde
        public string SourceDirectory { get; set; }   // Répertoire source des fichiers à sauvegarder
        public string TargetDirectory { get; set; }   // Répertoire cible où les fichiers seront sauvegardés
        public BackupType BackupType { get; set; }    // Type de sauvegarde : Complète ou Différentielle

        [JsonIgnore] // Exclure de la sérialisation
        public IBackupStrategy _backupStrategy;

        /// <summary>
        /// Constructeur de la classe BackupJob.
        /// Initialise un nouveau travail de sauvegarde avec les informations nécessaires.
        /// </summary>
        /// <param name="name">Nom du travail de sauvegarde.</param>
        /// <param name="sourceDirectory">Répertoire source des fichiers à sauvegarder.</param>
        /// <param name="targetDirectory">Répertoire cible où les fichiers seront sauvegardés.</param>
        /// <param name="backupType">Type de sauvegarde (Complète ou Différentielle).</param>
        public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType backupType)
        {
            // Vérification des paramètres pour éviter les valeurs nulles
            Name = name ?? throw new ArgumentNullException(nameof(name), "Le nom du travail de sauvegarde ne peut pas être nul.");
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory), "Le répertoire source ne peut pas être nul.");
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory), "Le répertoire cible ne peut pas être nul.");
            BackupType = backupType; // Le type de sauvegarde est attribué directement
            _backupStrategy = BackupStrategyFactory.GetStrategy(backupType);
        }

        /// <summary>
        /// Retourne une chaîne descriptive du travail de sauvegarde.
        /// Cette méthode est utile pour afficher des informations sur le travail de sauvegarde dans les logs ou l'interface utilisateur.
        /// </summary>
        /// <returns>Une chaîne contenant les informations du travail de sauvegarde (nom, source, cible, type).</returns>
        public override string ToString()
        {
            return $"Backup Job: {Name}, Source: {SourceDirectory}, Target: {TargetDirectory}, Type: {BackupType}";
        }
    }
}
