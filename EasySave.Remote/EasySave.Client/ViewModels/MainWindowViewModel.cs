using System;
using System.Reactive;
using ReactiveUI;
using System.Threading.Tasks;

namespace EasySave.Client.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> ClickCommand { get; }

        public MainWindowViewModel()
        {
            Console.WriteLine("✅ MainWindowViewModel loaded!");

            ClickCommand = ReactiveCommand.CreateFromTask(ClickFct);

        }

        private async Task ClickFct()
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Console.WriteLine("🎉 Button Clicked!");
            });
        }

    }
}
