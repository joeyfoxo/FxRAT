using System.Runtime.InteropServices;

namespace Lib;

public class WallpaperUtil
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPI_GETDESKWALLPAPER = 0x0073;
    private const int MAX_PATH = 260;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public static void ChangeWallpaper(string wallpaperPath)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, 0);
        Console.WriteLine("Wallpaper changed successfully.");
    }

    public static string GetWallpaperPath()
    {
        string wallpaperPath = new string(' ', MAX_PATH);
        SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, wallpaperPath, 0);
        return wallpaperPath.Substring(0, wallpaperPath.IndexOf('\0'));
    }

    public static void RestoreWallpaper()
    {
        string originalWallpaper = GetWallpaperPath();
        ChangeWallpaper(originalWallpaper);
    }
    
}