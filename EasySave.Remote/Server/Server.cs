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
    public class Server
    {
        private static readonly string ipAddressStr = "127.0.0.1";
        private static readonly int port = 11000;
        private static EasySaveFacade _facade = null!;
        private static IConfiguration _configuration = null!;
        private static bool _serverRunning = true;
        private static Socket _serverSocket = null!;

        public static void Start(IConfiguration configuration)
        {
            _configuration = configuration;
            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("../../EasySave.GUI/backup_jobs.json"),
                "Logs",
                null,
                _configuration
            );

            _serverSocket = InitializeServer();
            if (_serverSocket == null)
            {
                Console.WriteLine("Le serveur n'a pas pu démarrer.");
                return;
            }

            Console.WriteLine("Le serveur est démarré et attend des connexions...");

            while (_serverRunning)
            {
                try
                {
                    Socket clientSocket = _serverSocket.Accept();
                    Console.WriteLine($"Client connecté : {clientSocket.RemoteEndPoint}");
                    Task.Run(() => HandleClient(clientSocket));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'acceptation d'un client : {ex.Message}");
                }
            }

            ShutdownServer();
        }

        private static Socket InitializeServer()
        {
            try
            {
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(ipAddressStr);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine($"Serveur démarré sur {ipAddressStr}:{port}");
                return listener;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'initialisation du serveur : {ex.Message}");
                return null!;
            }
        }

        private static async Task HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0)
                        break;

                    string requestJson = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine($"Requête reçue : {requestJson}");
                    AnalyseRequest(requestJson, clientSocket);

                    if (requestJson.Trim().ToLower().Contains("disconnect"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du traitement du client : {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
                Console.WriteLine("Client déconnecté.");
            }
        }

        private static void AnalyseRequest(string requestJson, Socket socket)
        {
            try
            {
                var requestObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(requestJson);
                if (requestObj == null || !requestObj.ContainsKey("command"))
                {
                    SendResponse(socket, "Format de requête invalide.");
                    return;
                }

                string command = requestObj["command"].GetString()?.ToLower() ?? "";
                var parameters = requestObj.ContainsKey("parameters")
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(requestObj["parameters"].GetRawText())
                    : new Dictionary<string, object>();

                switch (command)
                {
                    case "execute":
                        ExecuteAllJobs(socket);
                        break;
                    case "list":
                        ListAllJobs(socket);
                        break;
                    case "pause":
                        HandleJobAction(socket, parameters, _facade.PauseJob, "Job {0} mis en pause.");
                        break;
                    case "resume":
                        HandleJobAction(socket, parameters, _facade.ResumeJob, "Job {0} repris.");
                        break;
                    case "stop":
                        HandleJobAction(socket, parameters, _facade.StopJob, "Job {0} arrêté.");
                        break;
                    default:
                        SendResponse(socket, "Commande inconnue.");
                        break;
                }
            }
            catch (Exception ex)
            {
                SendResponse(socket, $"Erreur lors du traitement de la requête : {ex.Message}");
            }
        }

        private static void HandleJobAction(Socket socket, Dictionary<string, object>? parameters, Action<Guid> jobAction, string successMessage)
        {
            if (parameters?.TryGetValue("jobId", out object jobIdObj) == true && Guid.TryParse(jobIdObj.ToString(), out Guid jobId))
            {
                jobAction(jobId);
                SendResponse(socket, string.Format(successMessage, jobId));
            }
            else
            {
                SendResponse(socket, "Erreur : Paramètre 'jobId' manquant ou invalide.");
            }
        }

        private static void ExecuteAllJobs(Socket socket)
        {
            Console.WriteLine("Exécution de tous les jobs de sauvegarde...");
            try
            {
                List<BackupJob> jobs = _facade.ListBackupJobs();
                if (jobs.Count == 0)
                {
                    SendResponse(socket, "Aucun job disponible.");
                    return;
                }

                BackupJob job = jobs[0];
                Guid jobId = job.Id;
                Console.WriteLine($"🎯 Lancement du job {jobId}...");

                Task.Run(() => _facade.ExecuteJobByIndex(0));

                SendResponse(socket, $"{{ \"jobId\": \"{jobId}\" }}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution des jobs : {ex.Message}");
                SendResponse(socket, "Erreur lors de l'exécution des jobs.");
            }
        }

        private static void ListAllJobs(Socket socket)
        {
            try
            {
                List<BackupJob> jobs = _facade.ListBackupJobs();
                string json = JsonSerializer.Serialize(jobs);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] size = BitConverter.GetBytes(data.Length);
                socket.Send(size);
                socket.Send(data);
                Console.WriteLine("Liste des jobs envoyée.");
            }
            catch (Exception ex)
            {
                SendResponse(socket, $"Erreur lors de l'envoi de la liste des jobs : {ex.Message}");
            }
        }

        private static void SendResponse(Socket socket, string message)
        {
            try
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(message);
                byte[] lengthBytes = BitConverter.GetBytes(responseBytes.Length);
                socket.Send(lengthBytes);
                socket.Send(responseBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la réponse : {ex.Message}");
            }
        }

        private static void StopServer()
        {
            Console.WriteLine("🛑 Arrêt du serveur...");
            _serverRunning = false;
            _serverSocket?.Close();
        }

        private static void ShutdownServer()
        {
            _serverSocket?.Close();
            _serverSocket = null!;
            Console.WriteLine("🔴 Le serveur a été arrêté.");
        }
    }
}
