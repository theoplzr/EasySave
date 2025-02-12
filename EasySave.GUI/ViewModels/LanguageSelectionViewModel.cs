using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using EasySave.GUI.Helpers;
using EasySave.GUI.Views;
using Avalonia.Controls.ApplicationLifetimes;

namespace EasySave.GUI.ViewModels
{
    public class LanguageSelectionViewModel : ViewModelBase
    {
        private readonly Window _window;

        public ReactiveCommand<Unit, Unit> SelectEnglishCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectFrenchCommand { get; }
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public LanguageSelectionViewModel(Window window)
        {
            _window = window;

            SelectEnglishCommand = ReactiveCommand.Create(() =>
            {
                LanguageHelper.Instance.SetLanguage("en");
                OpenMainWindow();
            });

            SelectFrenchCommand = ReactiveCommand.Create(() =>
            {
                LanguageHelper.Instance.SetLanguage("fr");
                OpenMainWindow();
            });
        }

        private void OpenMainWindow()
        {
            var mainWindow = new MainWindow();
            // Mettre à jour l'owner (MainWindow) dans l'ApplicationLifetime
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = mainWindow;
            }
            mainWindow.Show();
            _window.Close(); 
        }
    }
}
