using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    /// <summary>
    /// Represents the configuration window for EasySave settings.
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationWindow"/>.
        /// </summary>
        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = new ConfigurationViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Closes the window when called by a UI event.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
