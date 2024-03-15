namespace Server;

public class Files
{
    public static void writeToNotepad(string message)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.txt");

        // Write content to the file
        File.AppendAllText(filePath, message);
        
    }
}