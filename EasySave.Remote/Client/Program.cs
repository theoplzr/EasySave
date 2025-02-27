using Avalonia;
using Avalonia.ReactiveUI;

namespace EasySave.Remote.Client
{
    // Entry point of the application.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build and start the Avalonia application with classic desktop lifetime.
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Configures and builds the Avalonia application.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()   // Automatically detects the operating system.
                .UseReactiveUI()       // Uses ReactiveUI for MVVM support.
                .LogToTrace();       // Logs output to trace for debugging.
    }
}
