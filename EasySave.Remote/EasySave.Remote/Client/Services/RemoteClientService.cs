using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic; 

namespace EasySave.Remote.Client.Services
{
    /// <summary>
    /// Service de communication par socket avec le serveur EasySave.
    /// </summary>
    public class RemoteClientService
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private Socket? _clientSocket;

        public RemoteClientService(string serverIp = "127.0.0.1", int serverPort = 11000)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_serverIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _serverPort);
                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await _clientSocket.ConnectAsync(remoteEP);
                return _clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion : {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
        }

        public async Task<string> SendCommandAsync(string command, object? parameters = null)
        {
            if (_clientSocket == null)
                throw new InvalidOperationException("Non connecté.");

            var messageObj = new { command, parameters };
            string messageJson = JsonSerializer.Serialize(messageObj);
            byte[] msg = Encoding.UTF8.GetBytes(messageJson);

            await _clientSocket.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);

            // Lire la taille du message de réponse (4 octets)
            byte[] sizeBuffer = new byte[4];
            int sizeReceived = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(sizeBuffer), SocketFlags.None);
            if (sizeReceived == 0)
                return string.Empty;
            int dataSize = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] dataBuffer = new byte[dataSize];
            int totalReceived = 0;
            while (totalReceived < dataSize)
            {
                int bytesRead = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(dataBuffer, totalReceived, dataSize - totalReceived), SocketFlags.None);
                if (bytesRead == 0)
                    break;
                totalReceived += bytesRead;
            }
            return Encoding.UTF8.GetString(dataBuffer, 0, totalReceived);
        }
    }
}
