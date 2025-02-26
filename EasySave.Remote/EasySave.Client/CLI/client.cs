using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EasySave.Core.Models;
using System.Collections.Generic;
using System.Text.Json;


namespace EasySaveClient.CLI
{
    public class client
    {
        private static string ipAddressStr = "127.0.0.1";
        private static int port = 11000;
        static void Main(string[] args)
        {
            Socket mySocket = ConnectToServer();
            ListenToServer(mySocket);
            Disconnect(mySocket);
        }

        private static Socket ConnectToServer()
        {
            IPAddress ipAddress = IPAddress.Parse(ipAddressStr);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect(remoteEP);
                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return client;
        }

        private static void ListenToServer(Socket client)
        {
            byte[] bytes = new byte[1024];
            string message;

            while (true)
            {
                Console.Write("Client: ");
                message = Console.ReadLine();
                byte[] msg = Encoding.UTF8.GetBytes(message);

                client.Send(msg);

                if (message.IndexOf("<EOF>") > -1)
                {
                break;
                }

                // Lire la taille du message
                byte[] sizeBuffer = new byte[4];
                int receivedBytes = client.Receive(sizeBuffer);
                if (receivedBytes == 0) break; // Fin de connexion

                int dataSize = BitConverter.ToInt32(sizeBuffer, 0);

                // Lire les données selon la taille spécifiée
                byte[] dataBuffer = new byte[dataSize];
                receivedBytes = client.Receive(dataBuffer);
                if (receivedBytes == 0) break; // Fin de connexion

                // Convertir en chaîne JSON
                string json = Encoding.UTF8.GetString(dataBuffer);

                // Déterminer le type de message
                switch (message.ToLower())
                {
                    case "list":
                        HandleBackupJobsList(json);
                        break;
                    case "execute":
                        //HandleExecutionStatus(json);
                        break;
                    default:
                        Console.WriteLine($"Unknown response for '{message}': {json}");
                        break;
                }

                if (json.Contains("<EOF>")) break;
            

            }
        }

        private static void Disconnect(Socket socket)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Socket disconnected.");
            }
        }

        // Fonction pour traiter la liste des backup jobs
        private static void HandleBackupJobsList(string json)
        {
            List<BackupJob> jobs = JsonSerializer.Deserialize<List<BackupJob>>(json);
            
            Console.WriteLine("Backup Jobs:");
            foreach (var job in jobs)
            {
                Console.WriteLine($"{job}");
            }
        }

        // Fonction pour traiter la réponse d'exécution
        /*private static void HandleExecutionStatus(string json)
        {
            Console.WriteLine("🚀 Processing Execution Status...");
            var executionResult = JsonSerializer.Deserialize<ExecutionStatus>(json);

            Console.WriteLine($"🔹 Status: {executionResult.Status}");
            Console.WriteLine($"🔹 Message: {executionResult.Message}");
        }*/
        
    }    
}
