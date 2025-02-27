using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EasySave.Remote.Client.Services
{
    // This class handles the remote client operations, including connecting to the server
    // and sending commands.
    public class RemoteClientService
    {
        // Server IP address and port number.
        private readonly string _serverIp;
        private readonly int _serverPort;
        
        // The socket used for the client connection.
        private Socket? _clientSocket;

        // Constructor with default server IP and port.
        public RemoteClientService(string serverIp = "127.0.0.1", int serverPort = 11000)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        // Connects to the server asynchronously.
        public async Task<bool> ConnectAsync()
        {
            try
            {
                // Parse the server IP address.
                IPAddress ipAddress = IPAddress.Parse(_serverIp);
                // Create an endpoint with the IP address and port.
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _serverPort);

                // Initialize the socket with the appropriate address family and protocol.
                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                // Connect to the server endpoint asynchronously.
                await _clientSocket.ConnectAsync(remoteEP);
                
                // Return true if the socket is connected.
                return _clientSocket.Connected;
            }
            catch (Exception ex)
            {
                // Log the connection error.
                Console.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
        }

        // Disconnects from the server.
        public void Disconnect()
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                // Shutdown both send and receive on the socket, then close it.
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
        }

        // Sends a command to the server with optional parameters and returns the response.
        public async Task<string> SendCommandAsync(string command, object? parameters = null)
        {
            if (_clientSocket == null)
                throw new InvalidOperationException("Not connected to the server.");

            // Create an anonymous object containing the command and its parameters.
            var messageObj = new { command, parameters };
            // Serialize the message object to JSON.
            string messageJson = JsonSerializer.Serialize(messageObj);
            
            Console.WriteLine($"Sending command: {messageJson}");

            // Convert the JSON string into a UTF8 byte array.
            byte[] msg = Encoding.UTF8.GetBytes(messageJson);
            // Send the command to the server asynchronously.
            await _clientSocket.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);

            // Read the size of the incoming response message (first 4 bytes).
            byte[] sizeBuffer = new byte[4];
            int sizeReceived = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(sizeBuffer), SocketFlags.None);
            if (sizeReceived == 0)
                return string.Empty;
            
            // Convert the first 4 bytes into an integer representing the total data size.
            int dataSize = BitConverter.ToInt32(sizeBuffer, 0);

            // Create a buffer to hold the complete response.
            byte[] dataBuffer = new byte[dataSize];
            int totalReceived = 0;
            // Continue receiving data until the entire response is read.
            while (totalReceived < dataSize)
            {
                int bytesRead = await _clientSocket.ReceiveAsync(new ArraySegment<byte>(dataBuffer, totalReceived, dataSize - totalReceived), SocketFlags.None);
                if (bytesRead == 0)
                    break;
                totalReceived += bytesRead;
            }

            // Convert the received bytes back into a string.
            string response = Encoding.UTF8.GetString(dataBuffer, 0, totalReceived);
            Console.WriteLine($"Received response: {response}");
            return response;
        }
    }
}
