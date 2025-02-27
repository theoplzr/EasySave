using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.Core.Repositories;

namespace EasySave.Remote.Server
{
    // The Server class encapsulates all logic for starting, running,
    // and managing the remote server that processes client commands.
    public class Server
    {
        // Static fields for server configuration.
        private static readonly string ipAddressStr = "127.0.0.1";
        private static readonly int port = 11000;
        private static EasySaveFacade _facade = null!;
        private static IConfiguration _configuration = null!;
        private static bool _serverRunning = true;
        private static Socket _serverSocket = null!;

        // Start method initializes and runs the server.
        public static void Start(IConfiguration configuration)
        {
            // Store the configuration for later use.
            _configuration = configuration;
            // Initialize the facade with required repositories and configuration.
            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("../../EasySave.GUI/backup_jobs.json"),
                "Logs",
                null,
                _configuration
            );

            // Initialize the server socket.
            _serverSocket = InitializeServer();
            if (_serverSocket == null)
            {
                Console.WriteLine("Server failed to start.");
                return;
            }

            Console.WriteLine("Server started and waiting for connections...");

            // Main loop to accept incoming client connections.
            while (_serverRunning)
            {
                try
                {
                    // Accept a new client connection.
                    Socket clientSocket = _serverSocket.Accept();
                    Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");
                    // Handle the client connection asynchronously.
                    Task.Run(() => HandleClient(clientSocket));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while accepting a client: {ex.Message}");
                }
            }

            // Once the loop exits, shut down the server.
            ShutdownServer();
        }

        // Initializes the server socket for listening to client connections.
        private static Socket InitializeServer()
        {
            try
            {
                // Create a new socket for listening.
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Parse the IP address.
                IPAddress ipAddress = IPAddress.Parse(ipAddressStr);
                // Create an endpoint with the specified IP and port.
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                // Bind the socket to the endpoint.
                listener.Bind(localEndPoint);
                // Start listening with a backlog of 10 connections.
                listener.Listen(10);
                Console.WriteLine($"Server started on {ipAddressStr}:{port}");
                return listener;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing server: {ex.Message}");
                return null!;
            }
        }

        // Handles an individual client connection.
        private static async Task HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            try
            {
                // Continuously receive data from the client.
                while (true)
                {
                    // Receive data into the buffer.
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0)
                        break;

                    // Convert the received bytes into a string.
                    string requestJson = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine($"Received request: {requestJson}");
                    // Process the client request.
                    AnalyseRequest(requestJson, clientSocket);

                    // If the request contains "disconnect", break the loop.
                    if (requestJson.Trim().ToLower().Contains("disconnect"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client: {ex.Message}");
            }
            finally
            {
                // Close the client connection.
                clientSocket.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        // Analyzes and processes the JSON request from the client.
        private static void AnalyseRequest(string requestJson, Socket socket)
        {
            try
            {
                // Deserialize the JSON request into a dictionary.
                var requestObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(requestJson);
                if (requestObj == null || !requestObj.ContainsKey("command"))
                {
                    SendResponse(socket, "Invalid request format.");
                    return;
                }

                // Retrieve the command and convert it to lowercase.
                string command = requestObj["command"].GetString()?.ToLower() ?? "";
                // Deserialize any parameters if available.
                var parameters = requestObj.ContainsKey("parameters")
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(requestObj["parameters"].GetRawText())
                    : new Dictionary<string, object>();

                // Execute corresponding actions based on the command.
                switch (command)
                {
                    case "execute":
                        ExecuteAllJobs(socket);
                        break;
                    case "list":
                        ListAllJobs(socket);
                        break;
                    case "pause":
                        HandleJobAction(socket, parameters, _facade.PauseJob, "Job {0} paused.");
                        break;
                    case "resume":
                        HandleJobAction(socket, parameters, _facade.ResumeJob, "Job {0} resumed.");
                        break;
                    case "stop":
                        HandleJobAction(socket, parameters, _facade.StopJob, "Job {0} stopped.");
                        break;
                    default:
                        SendResponse(socket, "Unknown command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                SendResponse(socket, $"Error processing request: {ex.Message}");
            }
        }

        // Handles job-related actions (pause, resume, stop) based on parameters.
        private static void HandleJobAction(Socket socket, Dictionary<string, object>? parameters, Action<Guid> jobAction, string successMessage)
        {
            // Check if the 'jobId' parameter is provided and valid.
            if (parameters?.TryGetValue("jobId", out object jobIdObj) == true && Guid.TryParse(jobIdObj.ToString(), out Guid jobId))
            {
                // Execute the job action (pause, resume, stop).
                jobAction(jobId);
                // Send a success message with the job ID.
                SendResponse(socket, string.Format(successMessage, jobId));
            }
            else
            {
                SendResponse(socket, "Error: 'jobId' parameter is missing or invalid.");
            }
        }

        // Executes all backup jobs by starting the first job in the list.
        private static void ExecuteAllJobs(Socket socket)
        {
            Console.WriteLine("Executing all backup jobs...");
            try
            {
                // Retrieve the list of backup jobs from the facade.
                List<BackupJob> jobs = _facade.ListBackupJobs();
                if (jobs.Count == 0)
                {
                    SendResponse(socket, "No job available.");
                    return;
                }

                // For demonstration, execute only the first job.
                BackupJob job = jobs[0];
                Guid jobId = job.Id;
                Console.WriteLine($"Starting job {jobId}...");

                // Run the job asynchronously.
                Task.Run(() => _facade.ExecuteJobByIndex(0));

                // Send the job ID back to the client in JSON format.
                SendResponse(socket, $"{{ \"jobId\": \"{jobId}\" }}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing jobs: {ex.Message}");
                SendResponse(socket, "Error executing jobs.");
            }
        }

        // Lists all backup jobs and sends them to the client.
        private static void ListAllJobs(Socket socket)
        {
            try
            {
                // Get the list of backup jobs.
                List<BackupJob> jobs = _facade.ListBackupJobs();
                // Serialize the job list to JSON.
                string json = JsonSerializer.Serialize(jobs);
                byte[] data = Encoding.UTF8.GetBytes(json);
                // Send the size of the data first.
                byte[] size = BitConverter.GetBytes(data.Length);
                socket.Send(size);
                // Then send the actual data.
                socket.Send(data);
                Console.WriteLine("Job list sent.");
            }
            catch (Exception ex)
            {
                SendResponse(socket, $"Error sending job list: {ex.Message}");
            }
        }

        // Sends a response message to the client.
        private static void SendResponse(Socket socket, string message)
        {
            try
            {
                // Convert the response message into bytes.
                byte[] responseBytes = Encoding.UTF8.GetBytes(message);
                // First, send the length of the response.
                byte[] lengthBytes = BitConverter.GetBytes(responseBytes.Length);
                socket.Send(lengthBytes);
                // Then send the response itself.
                socket.Send(responseBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response: {ex.Message}");
            }
        }

        // Stops the server by setting the running flag to false and closing the server socket.
        private static void StopServer()
        {
            Console.WriteLine("Stopping server...");
            _serverRunning = false;
            _serverSocket?.Close();
        }

        // Shuts down the server and cleans up resources.
        private static void ShutdownServer()
        {
            _serverSocket?.Close();
            _serverSocket = null!;
            Console.WriteLine("Server has been stopped.");
        }
    }
}
