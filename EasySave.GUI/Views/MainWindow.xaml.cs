using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EasySave.Core.Models;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    /// <summary>
    /// Represents the main window of the EasySave GUI, displaying jobs and menu options.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Updates the SelectedJob in the ViewModel based on the button's data context.
        /// </summary>
        /// <param name="sender">The sender button.</param>
        /// <param name="e">The event arguments.</param>
        private void JobButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupJob job)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.SelectedJob = job;
                }
            }
        }
    }
}
