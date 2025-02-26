using Avalonia.Controls;
using EasySave.Client.ViewModels;

namespace EasySave.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(); 
        }
    }
}
