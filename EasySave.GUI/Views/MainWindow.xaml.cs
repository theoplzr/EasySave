using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.GUI.ViewModels;

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
    }
}
