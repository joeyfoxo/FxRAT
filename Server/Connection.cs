using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lib;

namespace Server
{
    public class Connection
    {
        private static Socket listener;
        private static readonly object fileLock = new object();

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

                    // Start a new task to handle the client asynchronously
                    _ = HandleClientAsync(handler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                listener?.Close();
            }
        }

        private static async Task HandleClientAsync(Socket handler)
        {
            try
            {
                byte[] buffer = new byte[1024];
                StringBuilder dataBuilder = new StringBuilder();

                while (true)
                {
                    // Receive data from the client
                    int bytesRead = await handler.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    if (bytesRead > 0)
                    {
                        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {dataReceived}");
                        dataBuilder.Append(dataReceived);

                        // Echo back the received data
                        await handler.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), SocketFlags.None);

                        // Write received data to file
                        lock (fileLock)
                        {
                            File.AppendAllText("data.txt", dataReceived);
                        }
                    }
                    else
                    {
                        // If no bytes are received, break out of the loop
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling client: " + ex.Message);
            }
            finally
            {
                // Close the socket
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}
