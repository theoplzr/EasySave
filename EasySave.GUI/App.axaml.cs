using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.GUI.Views;

namespace EasySave.GUI
{
    /// <summary>
    /// The Avalonia <see cref="Application"/> definition for EasySave's GUI.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Loads the XAML for the application.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Handles post-initialization logic, setting the main window for desktop-style application lifetimes.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Set the main window to the primary application window
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
