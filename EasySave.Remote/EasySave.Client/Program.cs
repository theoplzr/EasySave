using Avalonia;
using System;

namespace EasySave.Client
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<RemoteApp>()
                .UsePlatformDetect()
                .LogToTrace();
        }
    }
}
