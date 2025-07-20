using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace BazthalLib.Systems
{
    public class SysInfo
    {
        /// <summary>
        /// Retrieves detailed information about the CPU, including name, number of cores, thread count, and maximum
        /// clock speed.
        /// </summary>
        /// <remarks>This method queries the system's management information to gather CPU details. It is
        /// specific to Windows operating systems and relies on the Win32_Processor class.</remarks>
        /// <returns>A string containing the CPU information formatted with details such as CPU name, number of cores, thread
        /// count, and maximum clock speed in MHz.</returns>
        public static string GetCPUInfo()
        {
            StringBuilder sb = new StringBuilder();


            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sb.AppendLine($"CPU Name: {obj["Name"]}");
                    sb.AppendLine($"Cores: {obj["NumberOfCores"]}");
                    sb.AppendLine($"Threads: {obj["ThreadCount"]}");
                    sb.AppendLine($"Max Clock Speed: {obj["MaxClockSpeed"]} MHz");
                    sb.AppendLine();
                }


            }
            return sb.ToString();
        }

        /// <summary>
        /// Retrieves the total physical memory available on the system in gigabytes.
        /// </summary>
        /// <remarks>This method queries the system's physical memory using the Win32_PhysicalMemory
        /// class. The result is the sum of the capacity of all physical memory modules, converted to
        /// gigabytes.</remarks>
        /// <returns>The total physical memory in gigabytes.</returns>
        public static long GetMemoryInfo()
        {
            DebugUtils.Log("SystemInfo", "GetMemoryInfo", "\nMemory Info:");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory"))
            {
                long totalMemory = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalMemory += Convert.ToInt64(obj["Capacity"]);
                }
                DebugUtils.Log("SystemInfo", "GetMemoryInfo", $"Total RAM: {totalMemory / (1024 * 1024 * 1024)} GB");
                return totalMemory / (1024 * 1024 * 1024);
            }
        }

        /// <summary>
        /// Retrieves information about all logical drives on the system with a drive type of 3 (local disk).
        /// </summary>
        /// <remarks>This method queries the system for logical drives and formats the information into a
        /// human-readable string for each drive. The method only includes drives that are recognized as local
        /// disks.</remarks>
        /// <returns>A list of strings, each containing details about a local drive, including its device ID, file system, total
        /// size in gigabytes, and free space in gigabytes.</returns>
        public static List<string> GetStorageInfo()
        {
            List<string> drives = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_LogicalDisk where DriveType=3"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    long totalSize = Convert.ToInt64(obj["Size"]) / (1024 * 1024 * 1024);
                    long freeSpace = Convert.ToInt64(obj["FreeSpace"]) / (1024 * 1024 * 1024);
                    string driveInfo = $"Drive: {obj["DeviceID"]} | File System: {obj["FileSystem"]} | Total: {totalSize} GB | Free: {freeSpace} GB";
                    drives.Add(driveInfo);
                }
            }
            return drives;
        }
        /// <summary>
        /// Determines whether the specified drive has sufficient free space.
        /// </summary>
        /// <remarks>This method queries the system for the specified drive's free space and compares it
        /// to the required space. If the drive is not found, the method returns <see langword="false"/>.</remarks>
        /// <param name="driveLetter">The letter of the drive to check, such as "C".</param>
        /// <param name="requiredSpaceGB">The amount of free space required, in gigabytes.</param>
        /// <returns><see langword="true"/> if the drive has at least the specified amount of free space; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool HasEnoughSpace(string driveLetter, long requiredSpaceGB)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"select * from Win32_LogicalDisk where DeviceID='{driveLetter}:'"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    long freeSpace = Convert.ToInt64(obj["FreeSpace"]) / (1024 * 1024 * 1024);
                    return freeSpace >= requiredSpaceGB;
                }
            }
            return false; // Return false if drive isn't found
        }

        /// <summary>
        /// Retrieves information about the installed GPU(s) on the system.
        /// </summary>
        /// <remarks>This method queries the system's video controllers and returns details such as the
        /// GPU name and video memory. The information is formatted as a string with each GPU's details on separate
        /// lines.</remarks>
        /// <returns>A string containing the name and video memory of each GPU installed on the system.  Each GPU's information
        /// is separated by a newline.</returns>
        public static string GetGPUInfo()
        {
            StringBuilder sb = new StringBuilder();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sb.AppendLine($"GPU Name: {obj["Name"]}");
                    sb.AppendLine($"Video Memory: {Convert.ToInt64(obj["AdapterRAM"]) / (1024 * 1024)} MB");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Retrieves the current color mode setting for Windows applications or the system.
        /// </summary>
        /// <remarks>This method accesses the Windows registry to determine the color mode setting. It
        /// returns <see langword="1"/> if an error occurs during the registry access.</remarks>
        /// <param name="GetSystemColorModeInstead">If <see langword="true"/>, retrieves the system color mode; otherwise, retrieves the application color mode.</param>
        /// <returns>An integer representing the color mode: <see langword="0"/> for dark mode, <see langword="1"/> for light
        /// mode, or <see langword="-1"/> if the setting cannot be determined.</returns>
        public static int GetWindowsColorMode(bool GetSystemColorModeInstead = false)
        {
            try
            {
                return (int)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    GetSystemColorModeInstead ? "SystemUsesLightTheme" : "AppsUseLightTheme",
                    -1);
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Extracts the last folder name from a given file path.
        /// </summary>
        /// <param name="path">The file path from which to extract the last folder name. Can contain either backslashes or forward slashes
        /// as separators.</param>
        /// <returns>The name of the last folder in the specified path, or an empty string if the path is null, empty, or does
        /// not contain any folder names.</returns>
        public static string ExtractLastFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            string[] parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
        }
    }
}
