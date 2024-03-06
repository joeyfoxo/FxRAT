using System.Net;
using System.Net.Sockets;
using System.Text;
using Lib;

namespace Server
{
    public class Connection
    {
        private static Socket listener;
        private static StreamWriter fileWriter;

        public static void InitListener()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(Settings.host);
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(new IPEndPoint(ipAddress, Settings.port));
                Console.WriteLine("Server is listening on port " + Settings.port);
                listener.Listen(10);
                
                while (true)
                {
                    // Accept a connection and create a new socket for communication
                    Socket handler = listener.Accept();
                    Console.WriteLine("Client connected.");

                    // Receive data from the client
                    byte[] buffer = new byte[1024];
                    int bytesRead = handler.Receive(buffer);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {dataReceived}");

                    // Echo back the received data
                    byte[] response = Encoding.ASCII.GetBytes(dataReceived);
                    handler.Send(response);

                    // Close the socket
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                fileWriter?.Close();
                listener?.Close();
            }
        }
    }
}