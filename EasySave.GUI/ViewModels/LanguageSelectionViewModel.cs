using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using EasySave.GUI.Helpers;
using EasySave.GUI.Views;
using ReactiveUI;
using System;
using System.Reactive;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for a window that allows the user to select the application's display language.
    /// </summary>
    public class LanguageSelectionViewModel : ViewModelBase
    {
        private readonly Window _window;

        /// <summary>
        /// Command to set the language to English.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectEnglishCommand { get; }

        /// <summary>
        /// Command to set the language to French.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectFrenchCommand { get; }

        /// <summary>
        /// Provides access to localized messages.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageSelectionViewModel"/> class,
        /// setting up commands to select English or French.
        /// </summary>
        /// <param name="window">The window in which this ViewModel is being used.</param>
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

        /// <summary>
        /// Opens the primary application window after a language selection is made.
        /// </summary>
        private void OpenMainWindow()
        {
            var mainWindow = new MainWindow();
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = mainWindow;
            }
            mainWindow.Show();
            _window.Close();
        }
    }
}
