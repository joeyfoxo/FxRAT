using System.Text;

namespace Lib;

public class Settings
{
    public static string host = "127.0.0.1";
    public static int port = 8888;
    public static byte[] hashKey;
    public static byte[] salt = Encoding.UTF8.GetBytes("joeyfoxo");
}
