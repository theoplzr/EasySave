using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using System.Text.Json;


/// <summary>
/// Server class responsible for handling client connections via sockets.
/// It supports remote execution, job listing, and real-time command processing.
/// </summary>
public class Server
{
    private static string ipAddressStr = "127.0.0.1";
    private static int port = 11000;
    private static EasySaveFacade _facade;
    private static IConfiguration _configuration;
    private static bool _serverRunning = true; // Keeps the server running
    private static Socket _serverSocket; // Stores the main server socket

    /// <summary>
    /// Main entry point of the server. Initializes the configuration and starts listening for client connections.
    /// </summary>
    static void Main(string[] args)
    {
        // Initialize configuration
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
            .Build();

        // Initialize the EasySave facade
        _facade = new EasySaveFacade(
            new JsonBackupJobRepository("backup_jobs.json"),
            "Logs",
            null,
            _configuration
        );

        // Start the server loop
        _serverSocket = StartServer();
        if (_serverSocket == null) return;

        while (_serverRunning)
        {
            Socket clientSocket = AcceptConnection(_serverSocket);
            if (clientSocket != null)
            {
                ListenToClient(clientSocket);
                clientSocket.Close(); // Close the client connection after processing
            }
        }

        Console.WriteLine("Server stopped.");
    }

    /// <summary>
    /// Initializes and starts the server socket.
    /// </summary>
    /// <returns>The socket listener instance.</returns>
    private static Socket StartServer()
    {
        try
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(ipAddressStr);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            listener.Bind(localEndPoint);
            listener.Listen(10);
            Console.WriteLine($"Server started on {ipAddressStr}:{port}");
            return listener;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error starting server: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Accepts an incoming client connection.
    /// </summary>
    /// <param name="socket">The server socket.</param>
    /// <returns>The accepted client socket.</returns>
    private static Socket AcceptConnection(Socket socket)
    {
        try
        {
            Socket clientSocket = socket.Accept();
            IPEndPoint remoteEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Client connected from {remoteEndPoint.Address}:{remoteEndPoint.Port}");
            return clientSocket;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error accepting connection: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Listens to client requests and processes them accordingly.
    /// </summary>
    /// <param name="client">The client socket.</param>
    private static void ListenToClient(Socket client)
    {
        try
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int numByte = client.Receive(buffer);
                if (numByte == 0) break; // Client disconnected

                string request = Encoding.UTF8.GetString(buffer, 0, numByte);
                
                if (!string.IsNullOrEmpty(request))
                {
                    AnalyseRequest(request.ToLower().Trim(), client);
                }

                // If "stop" command is received, break loop to disconnect client
                if (request.ToLower().Trim() == "stop")
                {
                    break;
                }
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine("⚠️ SocketException: " + se.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Exception: " + ex.Message);
        }
    }

    /// <summary>
    /// Analyzes and processes the client's request.
    /// </summary>
    /// <param name="request">The request command received from the client.</param>
    private static void AnalyseRequest(string request, Socket socket)
    {
        switch (request)
        {
            case "execute":
                ExecuteAllJobs();
                break;
            case "list":
                ListAllJob(socket);
                break;
            case "pause":
                
                PauseJob();
                break;
            case "stop":
                StopServer();
                break;
            case "play":
                Console.WriteLine("▶️ Play");
                break;
            default:
                Console.WriteLine("⚠️ Unknown command");
                break;
        }
    }

    /// <summary>
    /// Stops the server and closes all active connections.
    /// </summary>
    private static void StopServer()
    {
        Console.WriteLine("Stopping server...");
        _serverRunning = false; // Stops the server loop

        if (_serverSocket != null)
        {
            _serverSocket.Close();
            _serverSocket = null;
        }

        Console.WriteLine("Server successfully stopped.");
    }

    /// <summary>
    /// Executes all backup jobs asynchronously.
    /// </summary>
    private static void ExecuteAllJobs()
    {
        Console.WriteLine("Executing all backup jobs...");
        try
        {
            Task.Run(() => _facade.ExecuteAllJobs());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing jobs: " + ex.Message);
        }
    }

    /// <summary>
    /// Lists all registered backup jobs.
    /// </summary>
    private static void ListAllJob(Socket socket)
    {
        Console.WriteLine("📋 Sending backup jobs list to client...");

        // Récupérer la liste des jobs
        List<BackupJob> list = _facade.ListBackupJobs();

        // Sérialiser la liste en JSON
        string json = JsonSerializer.Serialize(list);

        // Convertir en bytes pour l'envoi via socket
        byte[] data = Encoding.UTF8.GetBytes(json);

        // Envoyer la taille du message d'abord (important pour le client)
        socket.Send(BitConverter.GetBytes(data.Length));

        // Envoyer les données
        socket.Send(data);
        
        Console.WriteLine("✅ Backup jobs list sent!");
    }

    private static void PauseJob(Guid Id)
    {
        _facade.PauseJob(Id);
    }
}
