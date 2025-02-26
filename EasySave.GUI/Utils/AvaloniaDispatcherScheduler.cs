using Avalonia.Threading;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace EasySave.GUI.Utils
{
    /// <summary>
    /// Implements <see cref="IScheduler"/> for scheduling tasks on Avalonia's UI thread.
    /// Provides a singleton <see cref="AvaloniaDispatcherScheduler.Instance"/>.
    /// </summary>
    public class AvaloniaDispatcherScheduler : IScheduler
    {
        /// <summary>
        /// Gets the single, thread-safe instance of <see cref="AvaloniaDispatcherScheduler"/>.
        /// </summary>
        public static AvaloniaDispatcherScheduler Instance { get; } = new AvaloniaDispatcherScheduler();

        /// <summary>
        /// Gets the current time, used by scheduling operations.
        /// </summary>
        public DateTimeOffset Now => DateTimeOffset.Now;

        /// <summary>
        /// Schedules an immediate action on Avalonia's UI thread.
        /// </summary>
        /// <typeparam name="TState">The type of the state used by the action.</typeparam>
        /// <param name="state">The state object passed to the action.</param>
        /// <param name="action">The action to execute on the Avalonia UI thread.</param>
        /// <returns>A disposable that can cancel the scheduled action.</returns>
        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var cancellation = new CancellationDisposable();

            // Post immediately to the Avalonia UI thread
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellation.Token.IsCancellationRequested)
                {
                    action(this, state);
                }
            });

            return cancellation;
        }

        /// <summary>
        /// Schedules an action on the Avalonia UI thread at a specific time (DateTimeOffset).
        /// </summary>
        /// <typeparam name="TState">The type of the state used by the action.</typeparam>
        /// <param name="state">The state object passed to the action.</param>
        /// <param name="dueTime">The future time at which to run the action.</param>
        /// <param name="action">The action to execute on the Avalonia UI thread.</param>
        /// <returns>A disposable that can cancel the scheduled action.</returns>
        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var cancellation = new CancellationDisposable();

            TimeSpan delay = dueTime - Now;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            // Use Task.Delay to wait for the timespan, then post the action to the UI thread
            Task.Delay(delay).ContinueWith(_ =>
            {
                if (!cancellation.Token.IsCancellationRequested)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (!cancellation.Token.IsCancellationRequested)
                        {
                            action(this, state);
                        }
                    });
                }
            });

            return cancellation;
        }

        /// <summary>
        /// Schedules an action on the Avalonia UI thread after a specified time span.
        /// </summary>
        /// <typeparam name="TState">The type of the state used by the action.</typeparam>
        /// <param name="state">The state object passed to the action.</param>
        /// <param name="dueTime">The time span after which to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A disposable that can cancel the scheduled action.</returns>
        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            // Redirect to the overload that takes a DateTimeOffset
            return Schedule(state, Now + dueTime, action);
        }
    }
}
