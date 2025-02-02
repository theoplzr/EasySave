//classe pour gérer le type de sauvegarde

namespace EasySaveApp.Models
{
    /// <summary>
    /// Enumération des types de sauvegarde.
    /// </summary>
    public enum BackupType
    {
        Complete,       // Sauvegarde complète
        Differential,    // Sauvegarde différentielle
    }
}
