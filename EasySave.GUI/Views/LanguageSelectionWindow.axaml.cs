using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
            DataContext = new LanguageSelectionViewModel(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
