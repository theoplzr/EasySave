using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using EasySave.Remote.Client.Services;

namespace EasySave.Remote.Client.ViewModels
{
    /// <summary>
    /// ViewModel pour l'interface de la console déportée.
    /// Permet de se connecter au serveur, d'envoyer des commandes et d'afficher les réponses.
    /// </summary>
    public class RemoteViewModel : ReactiveObject
    {
        private readonly RemoteClientService _clientService;
        private string _statusMessage = "Déconnecté";

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ListJobsCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeCommand { get; }

        public RemoteViewModel()
        {
            _clientService = new RemoteClientService();

            // Créer les commandes sans forcer explicitement l'outputScheduler.
            ConnectCommand = ReactiveCommand.CreateFromTask(ConnectAsync);
            ListJobsCommand = ReactiveCommand.CreateFromTask(ListJobsAsync);
            ExecuteCommand = ReactiveCommand.CreateFromTask(ExecuteJobsAsync);
            PauseCommand = ReactiveCommand.CreateFromTask(() => SendPauseCommandAsync(Guid.Empty));
            ResumeCommand = ReactiveCommand.CreateFromTask(() => SendResumeCommandAsync(Guid.Empty));
        }

        private async Task ConnectAsync()
        {
            bool connected = await _clientService.ConnectAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = connected ? "Connecté" : "Échec de la connexion";
            });
        }

        private async Task ListJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("list");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Liste des jobs reçue:\n{response}";
            });
        }

        private async Task ExecuteJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("execute");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Commande d'exécution envoyée:\n{response}";
            });
        }

        private async Task SendPauseCommandAsync(Guid jobId)
        {
            string response = await _clientService.SendCommandAsync("pause", new { jobId });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Commande pause envoyée:\n{response}";
            });
        }

        private async Task SendResumeCommandAsync(Guid jobId)
        {
            string response = await _clientService.SendCommandAsync("resume", new { jobId });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Commande reprise envoyée:\n{response}";
            });
        }
    }
}
