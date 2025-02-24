using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.GUI.ViewModels;
using Avalonia.Interactivity;

namespace EasySave.GUI.Views
{
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = new ConfigurationViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
