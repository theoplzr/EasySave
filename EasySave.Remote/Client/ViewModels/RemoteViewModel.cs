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
    // ViewModel for the remote client, handling UI actions and updating status.
    public class RemoteViewModel : ReactiveObject
    {
        // Instance of the remote client service to manage communication.
        private readonly RemoteClientService _clientService;
        
        // Status message to display in the UI.
        private string _statusMessage = "Client disconnected from server";
        // GUID to track the current job. Initially set to an empty GUID.
        private Guid _currentJobId = Guid.Empty;

        // Brush representing the current status color.
        private IBrush _statusColor = Brushes.Gray;
        public IBrush StatusColor
        {
            get => _statusColor;
            set => this.RaiseAndSetIfChanged(ref _statusColor, value);
        }

        // Property for the status message.
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        // Reactive commands for various operations.
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ListJobsCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        // Constructor initializes the client service and reactive commands.
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

        // Connects to the server and updates the UI based on the connection status.
        private async Task ConnectAsync()
        {
            bool connected = await _clientService.ConnectAsync();
            // Update the UI thread with connection status.
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = connected ? "Client connected to server!" : "Failed to connect to server.";
                StatusColor = connected ? Brushes.Green : Brushes.Red;
            });
        }

        // Requests the list of jobs from the server and updates the UI with the response.
        private async Task ListJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("list");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"📋 Job list:\n{response}";
                StatusColor = Brushes.Blue;
            });
        }

        // Executes jobs by sending the execute command and extracting the job ID from the response.
        private async Task ExecuteJobsAsync()
        {
            string response = await _clientService.SendCommandAsync("execute");

            // Extract the job ID from the server's response.
            Guid jobId = ExtractJobIdFromResponse(response);
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (jobId != Guid.Empty)
                {
                    _currentJobId = jobId;
                    StatusMessage = $"Job started! ID: {_currentJobId}\n{response}";
                    StatusColor = Brushes.Green;
                }
                else
                {
                    StatusMessage = "Error: Unable to retrieve job ID.";
                    StatusColor = Brushes.Red;
                }
            });
        }
        
        // Sends a pause command for the current job and updates the UI.
        private async Task PauseJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                // If no job is running, display an error message.
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Error: No job in progress.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            Console.WriteLine($"Sending pause command for job {_currentJobId}");
            string response = await _clientService.SendCommandAsync("pause", new { jobId = _currentJobId });

            // Update the UI to reflect that the job has been paused.
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusColor = Brushes.Orange;
            });
        }

        // Sends a resume command for the current job and updates the UI.
        private async Task ResumeJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                // If no job is running, display an error message.
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Error: No job in progress.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            Console.WriteLine($"Sending resume command for job {_currentJobId}");
            string response = await _clientService.SendCommandAsync("resume", new { jobId = _currentJobId });

            // Update the UI to reflect that the job has been resumed.
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusColor = Brushes.Green;
            });
        }

        // Sends a stop command for the current job, updates the UI, and resets the current job ID.
        private async Task StopJobAsync()
        {
            if (_currentJobId == Guid.Empty)
            {
                // If no job is running, display an error message.
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = "Error: No job in progress.";
                    StatusColor = Brushes.Orange;
                });
                return;
            }

            string response = await _clientService.SendCommandAsync("stop", new { jobId = _currentJobId });
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusMessage = $"Job {_currentJobId} stopped.\n{response}";
                StatusColor = Brushes.Red;
            });

            // Reset the current job ID after stopping the job.
            _currentJobId = Guid.Empty;
        }

        // Extracts the job ID from the JSON response received from the server.
        private Guid ExtractJobIdFromResponse(string response)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    // Attempt to retrieve the "jobId" property from the JSON document.
                    if (doc.RootElement.TryGetProperty("jobId", out JsonElement jobIdElement))
                    {
                        return Guid.Parse(jobIdElement.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors encountered during extraction.
                Console.WriteLine($"Error extracting jobId: {ex.Message}");
            }

            // Return an empty GUID if extraction fails.
            return Guid.Empty;
        }
    }
}
