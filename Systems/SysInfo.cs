using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace BazthalLib.Systems
{
    public class SysInfo
    {

        /// <summary>
        /// Retrieves detailed information about the CPU, including its name, core count, thread count, and clock speed.
        /// </summary>
        /// <remarks>The method accesses the Windows Registry to obtain the CPU name and clock speed. If
        /// the clock speed is unavailable, it will be indicated as "Unavailable" in the returned string.</remarks>
        /// <param name="asGHz">A boolean value indicating whether the clock speed should be returned in gigahertz (GHz). If <see
        /// langword="true"/>, the clock speed is formatted as GHz; otherwise, it is formatted as megahertz (MHz).</param>
        /// <returns>A string containing the CPU name, number of physical cores, number of threads, and clock speed.</returns>
        public static string GetCPUInfo(bool asGHz = false)
        {
            var sb = new System.Text.StringBuilder();
            string cpuName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0" , "ProcessorNameString" , "")?.ToString() ?? "Unknown";
                       
            object mhzObj = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0" , "~MHz" , null);
            string freqency = "Unavailable";

            if (mhzObj is int mhz)
            {
                freqency = asGHz ? $"{(mhz / 1000.0):F2} GHz" : $"{mhz} MHz";
            }


            sb.AppendLine($"CPU Name: {cpuName}");
            sb.AppendLine($"Cores: {GetPhysicalCoreCount()}");
            sb.AppendLine($"Threads: {Environment.ProcessorCount}");
            sb.AppendLine($"Clock: {freqency}");

            return sb.ToString();
        }
        
        /// <summary>
        /// Retrieves the total physical memory available on the system in gigabytes.
        /// </summary>
        /// <returns>The total physical memory in gigabytes. Returns -1 if the memory status cannot be determined.</returns>
        public static long GetMemoryInfo()
        {
            MEMORYSTATUSEX memStatus = new();

            if (GlobalMemoryStatusEx(memStatus))
            {
                return (long)Math.Round(memStatus.ullTotalPhys / 1024.0 / 1024 / 1024);
            }
            return -1;
        }
        
        /// <summary>
        /// Retrieves information about all fixed drives on the system that are ready for use.
        /// </summary>
        /// <remarks>This method only includes drives that are of type <see cref="DriveType.Fixed"/> and
        /// are ready for use. The size and free space are reported in gigabytes.</remarks>
        /// <returns>A list of strings, each containing details about a fixed drive, including its name, file system type, total
        /// size in gigabytes, and available free space in gigabytes. The list will be empty if no fixed drives are
        /// ready.</returns>
        public static List<string> GetStorageInfo()
        {
            var drives = new List<string>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    long totalSize = drive.TotalSize / (1024 * 1024 * 1024);
                    long freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    drives.Add($"Drive: {drive.Name} | File System: {drive.DriveFormat} | Total: {totalSize} GB | Free: {freeSpace} GB");
                }
            }
            return drives;
        }

        /// <summary>
        /// Determines whether the specified drive has sufficient available space.
        /// </summary>
        /// <remarks>This method checks if the drive is ready before determining the available space. If
        /// the drive is not ready or an error occurs, the method returns <see langword="false"/>.</remarks>
        /// <param name="driveLetter">The letter of the drive to check, such as "C".</param>
        /// <param name="requiredSpaceGB">The amount of space required, in gigabytes.</param>
        /// <returns><see langword="true"/> if the drive has more available space than the specified amount; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool HasEnoughSpace(string driveLetter, long requiredSpaceGB)
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    return freeGB > requiredSpaceGB;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Retrieves information about the GPU, including the card name and display memory.
        /// </summary>
        /// <remarks>This method executes the DirectX Diagnostic Tool (dxdiag) to gather GPU details and
        /// returns the relevant information as a string. It temporarily writes the dxdiag output to a file in the
        /// system's temporary directory, which is deleted after processing. If an error occurs during execution, the
        /// error message is included in the returned string.</remarks>
        /// <returns>A string containing the GPU card name and display memory information. If the information cannot be
        /// retrieved, the string will contain an error message.</returns>
        public static string GetGPUInfo()
        {
            var sb =  new System.Text.StringBuilder();
            try 
            {
                string dxdiagPath = Path.Combine(Path.GetTempPath(), "dxdiag.txt");
                var psi = new ProcessStartInfo("cmd", $"/c dxdiag /t \"{dxdiagPath}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var process = Process.Start(psi);
                process?.WaitForExit();

                if (File.Exists(dxdiagPath))
                {
                    foreach (var line in File.ReadLines(dxdiagPath))
                    {
                        if (line.Contains("Card name") || line.Contains("Display Memory"))
                            sb.AppendLine(line.Trim());
                    }
                    File.Delete(dxdiagPath);
                }
            }
            catch(Exception ex) { sb.AppendLine($"GPU Info is not available: {ex.Message}"); }

            return sb.ToString();
        }

        /// <summary>
        /// Retrieves the current color mode setting for Windows applications or the system.
        /// </summary>
        /// <remarks>This method accesses the Windows registry to determine the color mode setting. It
        /// returns 1 if an error occurs during retrieval.</remarks>
        /// <param name="GetSystemColorModeInstead">If <see langword="true"/>, retrieves the system color mode; otherwise, retrieves the application color mode.</param>
        /// <returns>An integer representing the color mode: 0 for dark mode, 1 for light mode, or -1 if the setting cannot be
        /// determined.</returns>
        public static int GetWindowsColorMode(bool GetSystemColorModeInstead = false)
        {
            try {
                return (int)(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",GetSystemColorModeInstead ? "SystemUsesLightTheme" : "AppsUseLightTheme", -1) ?? -1);
            }
            catch { return 1; }

        }

        /// <summary>
        /// Represents memory status information for the system.
        /// </summary>
        /// <remarks>This class provides detailed information about the system's current memory usage,
        /// including physical memory, page file, and virtual memory statistics. It is typically used to retrieve memory
        /// status data from the operating system.</remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        /// <summary>
        /// Retrieves information about the system's current usage of both physical and virtual memory.
        /// </summary>
        /// <remarks>This method is a P/Invoke wrapper for the Windows API function GlobalMemoryStatusEx.
        /// It provides detailed information about the memory status of the system, including the amount of physical and
        /// virtual memory available.</remarks>
        /// <param name="lpBuffer">A pointer to a <see cref="MEMORYSTATUSEX"/> structure that receives information about the current memory
        /// status.</param>
        /// <returns><see langword="true"/> if the function succeeds; otherwise, <see langword="false"/>.</returns>
        [DllImport("kernel32.dll" ,CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        /// <summary>
        /// Retrieves information about the logical processors in the system.
        /// </summary>
        /// <remarks>This function is a P/Invoke declaration for the Windows API function in kernel32.dll.
        /// It is used to obtain information about the system's logical processors, such as their relationship to
        /// physical processors, processor cores, and processor packages.</remarks>
        /// <param name="buffer">A pointer to a buffer that receives an array of <see cref="SYSTEM_LOGICAL_PROCESSOR_INFORMATION"/>
        /// structures.</param>
        /// <param name="returnLength">On input, specifies the size of the buffer in bytes. On output, receives the number of bytes returned in the
        /// buffer.</param>
        /// <returns><see langword="true"/> if the function succeeds; otherwise, <see langword="false"/>. If the function fails,
        /// call <see cref="Marshal.GetLastWin32Error"/> to obtain more information about the error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetLogicalProcessorInformation(
            IntPtr buffer,
            ref uint returnLength);

        /// <summary>
        /// Retrieves the number of physical processor cores on the current machine.
        /// </summary>
        /// <remarks>This method uses the Windows API to gather information about the logical processors
        /// and calculates the number of physical cores based on the processor relationship data.</remarks>
        /// <returns>The number of physical processor cores. Returns -1 if the information cannot be retrieved.</returns>
        public static int GetPhysicalCoreCount()
        {
            uint length = 0;
            GetLogicalProcessorInformation(IntPtr.Zero, ref length);

        IntPtr ptr = Marshal.AllocHGlobal((int)length);
            try
            {
                if (!GetLogicalProcessorInformation(ptr, ref length))
                    return -1;

                int size = Marshal.SizeOf(typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
                int count = (int)length / size;

                int physicalCoreCount = 0;
                for (int i = 0; i < count; i++)
                {
                    IntPtr itemPtr = new IntPtr(ptr.ToInt64() +  i * size);
                    var info = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(itemPtr);
                    if (info.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                        physicalCoreCount++;                   
                }
                return physicalCoreCount;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        /// <summary>
        /// Represents information about a logical processor in a system.
        /// </summary>
        /// <remarks>This structure provides details about the processor mask, the relationship of the
        /// processor to other processors, and additional processor-specific information. It is used to retrieve
        /// information about the logical processors in a system, which can be useful for optimizing application
        /// performance based on processor topology.</remarks>
        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
        {
            public UIntPtr ProcessorMask;
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public ProcessorInfoUnion ProcessorInfo;    
        }

        /// <summary>
        /// Represents a union of processor-related information, allowing access to different types of processor data.
        /// </summary>
        /// <remarks>This structure uses explicit layout to overlay different processor information types
        /// at the same memory location. It provides access to processor core details, NUMA node information, and cache
        /// descriptors.</remarks>
        [StructLayout(LayoutKind.Explicit)]
        struct ProcessorInfoUnion
        {
            [FieldOffset(0)] public ProcessorCore ProcessorCore;
            [FieldOffset(0)] public NumaNode NumaNode;
            [FieldOffset(0)] public cacheDescriptor Cache;
            [FieldOffset(0)] private ulong Reserved1;
            [FieldOffset(0)] private ulong Reserved2;       
        }

        /// <summary>
        /// Represents a core of a processor, encapsulating its operational flags.
        /// </summary>
        /// <remarks>The <see cref="ProcessorCore"/> structure is used to manage and represent the state
        /// of an individual processor core. The <see cref="Flags"/> field can be used to store various status or
        /// configuration flags relevant to the core's operation.</remarks>
        struct ProcessorCore
        {
            public byte Flags;
        }

        /// <summary>
        /// Represents a Non-Uniform Memory Access (NUMA) node in a system.
        /// </summary>
        /// <remarks>A NUMA node is a grouping of processors and memory that is optimized for local
        /// access. This structure is used to identify and work with specific NUMA nodes in a system.</remarks>
        struct NumaNode
        {
            public uint NodeNumber;
        }

        /// <summary>
        /// Represents a descriptor for a cache, detailing its level, associativity, line size, total size, and type.
        /// </summary>
        /// <remarks>This structure provides essential information about a cache's configuration, which
        /// can be used to understand its performance characteristics and suitability for specific tasks.</remarks>
        struct cacheDescriptor
        {
            public byte Level;
            public byte Associativity;
            public ushort LineSize;
            public uint Size;
            public int Type;
        }

        /// <summary>
        /// Specifies the relationship between logical processors and various system components.
        /// </summary>
        /// <remarks>This enumeration is used to identify the type of relationship that exists between
        /// logical processors and other elements such as processor cores, NUMA nodes, caches, processor packages, and
        /// groups.</remarks>
        enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore,
            RelationNumaNode,
            RelationCache,
            RelationProcessorPackage,
            RelationGroup,
            RelationAll,

        }
    }
}
