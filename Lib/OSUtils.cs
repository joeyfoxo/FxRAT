using System.Management;

namespace Lib;

public class OSUtils
{
    public static List<string> GetAntivirusSoftware()
    {
        List<string> antivirusList = new List<string>();

        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntivirusProduct");
            ManagementObjectCollection results = searcher.Get();

            foreach (ManagementObject result in results)
            {
                string displayName = result["displayName"] as string;
                antivirusList.Add(displayName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return antivirusList;
    }
    
    static void PrintInformation(string title, List<string> infoList)
        {
            Console.WriteLine(title);
            foreach (string info in infoList)
            {
                Console.WriteLine(info);
            }
            Console.WriteLine();
        }

        public static List<string> GetUserInformation()
        {
            List<string> userInfo = new List<string>();
            userInfo.Add($"Username: {Environment.UserName}");
            userInfo.Add($"Domain: {Environment.UserDomainName}");
            userInfo.Add($"User Profile Path: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
            return userInfo;
        }

        public static List<string> GetSystemInformation()
        {
            List<string> systemInfo = new List<string>();

            try
            {
                // Retrieve information about the operating system
                ManagementObjectSearcher osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject osObj in osSearcher.Get())
                {
                    systemInfo.Add($"Operating System: {osObj["Caption"]} {osObj["Version"]}");
                    systemInfo.Add($"Architecture: {osObj["OSArchitecture"]}");
                    systemInfo.Add($"Manufacturer: {osObj["Manufacturer"]}");
                    systemInfo.Add($"Serial Number: {osObj["SerialNumber"]}");
                }

                // Retrieve information about the processor
                ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject processorObj in processorSearcher.Get())
                {
                    systemInfo.Add($"Processor: {processorObj["Name"]}");
                    systemInfo.Add($"Cores: {processorObj["NumberOfCores"]}");
                    systemInfo.Add($"Logical Processors: {processorObj["NumberOfLogicalProcessors"]}");
                    systemInfo.Add($"Processor ID: {processorObj["ProcessorId"]}");
                }

                // Retrieve information about memory
                ManagementObjectSearcher memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                ulong totalMemoryBytes = 0;
                foreach (ManagementObject memoryObj in memorySearcher.Get())
                {
                    ulong memorySizeBytes = Convert.ToUInt64(memoryObj["Capacity"]);
                    totalMemoryBytes += memorySizeBytes;
                }
                double totalMemoryGB = Math.Round(totalMemoryBytes / (1024.0 * 1024.0 * 1024.0), 2);
                systemInfo.Add($"Total Memory: {totalMemoryGB} GB");

                
                ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"); // DriveType 3 is for local disks
                foreach (ManagementObject driveObj in driveSearcher.Get())
                {
                    string driveLetter = driveObj["DeviceID"] as string;
                    string driveLabel = driveObj["VolumeName"] as string ?? "Unknown";
                    ulong totalSizeBytes = Convert.ToUInt64(driveObj["Size"]);
                    ulong freeSpaceBytes = Convert.ToUInt64(driveObj["FreeSpace"]);
                    double totalSizeGB = Math.Round(totalSizeBytes / (1024.0 * 1024.0 * 1024.0), 2);
                    double freeSpaceGB = Math.Round(freeSpaceBytes / (1024.0 * 1024.0 * 1024.0), 2);
                    systemInfo.Add($"Drive {driveLetter} {driveLabel}");
                    systemInfo.Add($"Total Size: {totalSizeGB} GB");
                    systemInfo.Add($"Free Space: {freeSpaceGB} GB");
                }
                // Add more queries to retrieve additional system information if needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving system information: {ex.Message}");
                // Log or handle the error as needed
                systemInfo.Add($"Error retrieving system information: {ex.Message}");
            }

            return systemInfo;
        }

        public static List<string> GetInstalledSoftwareInformation()
        {
            List<string> softwareInfo = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
            foreach (ManagementObject obj in searcher.Get())
            {
                softwareInfo.Add($"Name: {obj["Name"]}");
                softwareInfo.Add($"Version: {obj["Version"]}");
            }
            return softwareInfo;
        }

        public static List<string> GetFileSystemInformation()
        {
            List<string> fileSystemInfo = new List<string>();
            fileSystemInfo.Add($"Downloads Folder: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads");
            fileSystemInfo.Add($"Recent Documents Folder: {Environment.GetFolderPath(Environment.SpecialFolder.Recent)}");
            return fileSystemInfo;
        }

        public static List<string> GetNetworkAdapterInformation()
        {
            List<string> networkInfo = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
            foreach (ManagementObject obj in searcher.Get())
            {
                networkInfo.Add($"MAC Address: {obj["MACAddress"]}");
                networkInfo.Add($"IP Address: {((string[])obj["IPAddress"])[0]}");
            }
            return networkInfo;
        }
}