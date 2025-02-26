using Avalonia.Media; 
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Text.Json; 

using EasySave.Remote.Client.Services;

namespace EasySave.Remote.Client.ViewModels
{
    public class RemoteViewModel : ReactiveObject
    {
        private readonly RemoteClientService _clientService;
        private string _statusMessage = "Client déconnecté du serveur";
        private Guid _currentJobId = Guid.Empty;  // ID du job en cours

        // ✅ Ajout de la propriété pour la couleur du statut
        private IBrush _statusColor = Brushes.Gray; 
        public IBrush StatusColor
        {
            get => _statusColor;
            set => this.RaiseAndSetIfChanged(ref _statusColor, value);
        }

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
        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public RemoteViewModel()
        {
            _clientService = new RemoteClientService();

            ConnectCommand = ReactiveCommand.CreateFromTask(ConnectAsync);
            ListJobsCommand = ReactiveCommand.CreateFromTask(ListJobsAsync);
            ExecuteCommand = ReactiveCommand.CreateFromTask(ExecuteJobsAsync);
            PauseCommand = ReactiveCommand.CreateFromTask(PauseJobAsync);
            ResumeCommand = ReactiveCommand.CreateFromTask(ResumeJobAsync);
            StopCommand = ReactiveCommand.CreateFromTask(StopJobAsync);
        }

        private async Task ConnectAsync()
        {
            bool connected = await _clientService.ConnectAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = connected ? "✅ Client connecté au serveur !" : "❌ Échec de connexion avec le serveur.";
                StatusColor = connected ? Brushes.Green : Brushes.Red;
            });
        }

        private async Task ListJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("list");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"📋 Liste des jobs :\n{response}";
                StatusColor = Brushes.Blue;
            });
        }

        private async Task ExecuteJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("execute");

            // Extraction de l'ID du job depuis la réponse du serveur
            Guid jobId = ExtractJobIdFromResponse(response);
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (jobId != Guid.Empty)
                {
                    _currentJobId = jobId;
                    StatusMessage = $"Job lancé ! ID: {_currentJobId}\n{response}";
                    StatusColor = Brushes.Green;
                }
                else
                {
                    StatusMessage = "Erreur : Impossible de récupérer l'ID du job.";
                    StatusColor = Brushes.Red;
                }
            });
        }
        private async Task PauseJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Erreur : Aucun job en cours.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            Console.WriteLine($"Envoi de la commande pause pour le job {_currentJobId}");
            string response = await _clientService.SendCommandAsync("pause", new { jobId = _currentJobId });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusColor = Brushes.Orange;
            });
        }

        private async Task ResumeJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Erreur : Aucun job en cours.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            Console.WriteLine($"Envoi de la commande reprise pour le job {_currentJobId}");
            string response = await _clientService.SendCommandAsync("resume", new { jobId = _currentJobId });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Job {_currentJobId} repris.\n{response}";
                StatusColor = Brushes.Green;
            });
        }
        private async Task StopJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Erreur : Aucun job en cours.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            string response = await _clientService.SendCommandAsync("stop", new { jobId = _currentJobId });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Job {_currentJobId} arrêté.\n{response}";
                StatusColor = Brushes.Red;
            });

            _currentJobId = Guid.Empty; // Réinitialisation après l'arrêt
        }

       private Guid ExtractJobIdFromResponse(string response)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    if (doc.RootElement.TryGetProperty("jobId", out JsonElement jobIdElement))
                    {
                        return Guid.Parse(jobIdElement.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'extraction de jobId: {ex.Message}");
            }

            return Guid.Empty; 
        }

    }
}
