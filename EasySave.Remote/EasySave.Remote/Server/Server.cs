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
        private static string ipAddressStr = "127.0.0.1";
        private static int port = 11000;
        private static EasySaveFacade _facade;
        private static IConfiguration _configuration;
        private static bool _serverRunning = true;
        private static Socket _serverSocket;

        public static void Start(IConfiguration configuration)
        {
            _configuration = configuration;
            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
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
                    Console.WriteLine("Client connecté : " + clientSocket.RemoteEndPoint);
                    Task.Run(() => HandleClient(clientSocket));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de l'acceptation d'un client : " + ex.Message);
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
                Console.WriteLine("Erreur lors de l'initialisation du serveur : " + ex.Message);
                return null;
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
                    Console.WriteLine("Requête reçue : " + requestJson);
                    AnalyseRequest(requestJson, clientSocket);

                    if (requestJson.Trim().ToLower().Contains("disconnect"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du traitement du client : " + ex.Message);
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
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                if (requestObj.ContainsKey("parameters"))
                {
                    parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestObj["parameters"].GetRawText());
                }

                switch (command)
                {
                    case "execute":
                        ExecuteAllJobs();
                        SendResponse(socket, "Commande d'exécution reçue.");
                        break;
                    case "list":
                        ListAllJobs(socket);
                        break;
                    case "pause":
                        if (parameters.TryGetValue("jobId", out object jobIdObj) && Guid.TryParse(jobIdObj.ToString(), out Guid jobId))
                        {
                            PauseJob(jobId);
                            SendResponse(socket, $"Job {jobId} mis en pause.");
                        }
                        else
                        {
                            SendResponse(socket, "Paramètre 'jobId' manquant ou invalide pour la commande pause.");
                        }
                        break;
                    case "resume":
                        if (parameters.TryGetValue("jobId", out object resumeJobIdObj) && Guid.TryParse(resumeJobIdObj.ToString(), out Guid resumeJobId))
                        {
                            ResumeJob(resumeJobId);
                            SendResponse(socket, $"Job {resumeJobId} repris.");
                        }
                        else
                        {
                            SendResponse(socket, "Paramètre 'jobId' manquant ou invalide pour la commande resume.");
                        }
                        break;
                    case "stop":
                        StopServer();
                        SendResponse(socket, "Arrêt du serveur.");
                        break;
                    default:
                        SendResponse(socket, "Commande inconnue.");
                        break;
                }
            }
            catch (Exception ex)
            {
                SendResponse(socket, "Erreur lors du traitement de la requête : " + ex.Message);
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
                Console.WriteLine("Erreur lors de l'envoi de la réponse : " + ex.Message);
            }
        }

        private static void ExecuteAllJobs()
        {
            Console.WriteLine("Exécution de tous les jobs de sauvegarde...");
            try
            {
                Task.Run(() => _facade.ExecuteAllJobs());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'exécution des jobs : " + ex.Message);
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
                SendResponse(socket, "Erreur lors de l'envoi de la liste des jobs : " + ex.Message);
            }
        }

        private static void PauseJob(Guid jobId)
        {
            Console.WriteLine($"Mise en pause du job {jobId}...");
            _facade.PauseJob(jobId);
        }

        private static void ResumeJob(Guid jobId)
        {
            Console.WriteLine($"Reprise du job {jobId}...");
            _facade.ResumeJob(jobId);
        }

        private static void StopServer()
        {
            Console.WriteLine("Arrêt du serveur...");
            _serverRunning = false;
            _serverSocket.Close();
        }

        private static void ShutdownServer()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _serverSocket = null;
            }
            Console.WriteLine("Le serveur a été arrêté.");
        }
    }
}
