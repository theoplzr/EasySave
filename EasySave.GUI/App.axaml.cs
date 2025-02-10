using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.GUI.Views;

namespace EasySave.GUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Afficher la fenêtre de sélection de langue avant d'ouvrir MainWindow
                var languageWindow = new LanguageSelectionWindow();
                languageWindow.Show();

                // Assurer que l'application ne se ferme pas immédiatement
                desktop.MainWindow = languageWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
