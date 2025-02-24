using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables; // Pour CancellationDisposable
using System.Threading.Tasks;
using Avalonia.Threading;

namespace EasySave.GUI.Utils
{
    public class AvaloniaDispatcherScheduler : IScheduler
    {
        // Instance unique (singleton)
        public static AvaloniaDispatcherScheduler Instance { get; } = new AvaloniaDispatcherScheduler();

        public DateTimeOffset Now => DateTimeOffset.Now;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var cancellation = new CancellationDisposable();

            // Exécuter immédiatement sur le thread UI d'Avalonia
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellation.Token.IsCancellationRequested)
                {
                    action(this, state);
                }
            });

            return cancellation;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var cancellation = new CancellationDisposable();

            // Calculer le délai nécessaire
            TimeSpan delay = dueTime - Now;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            // Utiliser Task.Delay pour attendre le délai, puis poster l'action sur le thread UI
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

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            // Rediriger vers l'overload avec DateTimeOffset
            return Schedule(state, Now + dueTime, action);
        }
    }
}
