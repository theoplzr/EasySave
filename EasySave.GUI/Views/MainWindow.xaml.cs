using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.GUI.ViewModels;
using Avalonia.Interactivity;
using EasySave.Core.Models;

namespace EasySave.GUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void JobButton_Click(object? sender, RoutedEventArgs e)
        {
            // Vérifie que sender est bien un Button
            if (sender is Button button)
            {
                // Le DataContext du bouton correspond à l'objet BackupJob
                if (button.DataContext is BackupJob job)
                {
                    // Vérifie que le DataContext de la fenêtre est bien un MainWindowViewModel
                    if (this.DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.SelectedJob = job;
                    }
                }
            }
        }
    }
}
