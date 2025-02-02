namespace EasySaveApp.Commands
{
    /// <summary>
    /// Interface pour définir la structure d'une commande.
    /// Chaque commande doit implémenter la méthode Execute().
    /// </summary>
    public interface ICommand
    {
        void Execute();
    }
}
