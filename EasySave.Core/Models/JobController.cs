using System.Threading;

namespace EasySave.Core.Models
{
    public class JobController
    {
        // Pour arrêter immédiatement le job
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        
        // Pour gérer la pause et la reprise (initialement en mode "play")
        public ManualResetEventSlim PauseEvent { get; } = new ManualResetEventSlim(true);
        
        // Etat actuel du job
        public JobState State { get; set; } = JobState.Running;
    }
}
