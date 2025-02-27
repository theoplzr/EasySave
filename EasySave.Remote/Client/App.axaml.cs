using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace EasySave.Remote.Client
{
    // Main application class for initializing the Avalonia app.
    public partial class App : Application
    {
        // Load the XAML for the application.
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        // Configure the main window for desktop applications.
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Set the main window.
                desktop.MainWindow = new Views.MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
