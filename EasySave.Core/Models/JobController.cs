using System.Threading;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Controls the state and flow of a backup job, including cancellation and pause/resume functionality.
    /// </summary>
    public class JobController
    {
        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> used to stop the job immediately.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        /// <summary>
        /// Gets the <see cref="ManualResetEventSlim"/> used to pause and resume the job.
        /// Initially set to <c>true</c>, meaning "running" mode.
        /// </summary>
        public ManualResetEventSlim PauseEvent { get; } = new ManualResetEventSlim(true);

        /// <summary>
        /// Gets or sets the current state of the job (Running, Paused, Stopped).
        /// Defaults to <see cref="JobState.Running"/>.
        /// </summary>
        public JobState State { get; set; } = JobState.Running;
    }
}
