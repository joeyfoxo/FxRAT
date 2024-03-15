using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FxRAT.Utils
{
    public class Connection
    {
        private Socket _clientSocket;
        private readonly IPAddress _serverIp;
        private readonly int _serverPort;

        public Connection(string serverIp, int serverPort)
        {
            _serverIp = IPAddress.Parse(serverIp);
            _serverPort = serverPort;
        }

        public void Connect()
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(new IPEndPoint(_serverIp, _serverPort));
                Console.WriteLine("Connected to server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to server: " + ex.Message);
            }
        }

        public void Write(string data)
        {
            try
            {
                byte[] dataBytes = Encoding.ASCII.GetBytes(data);
                _clientSocket.Send(dataBytes);
                Console.WriteLine("Data sent successfully.");
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending data: " + ex.Message);
                Reconnect();
                Write(data);
            }
        }

        private void Reconnect()
        {
            try
            {
                _clientSocket.Close();
                Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reconnecting to server: " + ex.Message);
            }
        }
    }
}