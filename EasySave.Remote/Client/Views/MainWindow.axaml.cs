using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.Remote.Client.Views
{
    // Code-behind for the MainWindow.
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Initialize XAML components.
            InitializeComponent();
        }

        // Loads the XAML for the window.
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
