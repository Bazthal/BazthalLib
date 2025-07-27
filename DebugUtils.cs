using System;
using System.ComponentModel;
using System.Linq;
using BazthalLib.Systems.IO;

namespace BazthalLib
{
    public class DebugUtils
    {
        // Global declaration for debug mode to toggle debugger console output in Visual Studio (Untested anywhere else)
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool DebugMode { get; set; } = false;
        public static bool LogtoFile { get; set; } = false;
        public static long MaxSizeBytes { get; set; } = 5 * 1024 * 1024;
        public static int MaxBackUps { get; set; } = 5;
        private static readonly string LogFile = "Logs\\debug.log";

        /// <summary>
        /// Logs a message to the debug output with an optional timestamp.
        /// </summary>
        /// <remarks>The method logs messages only when the application is in debug mode. If logging to a
        /// file is enabled, the message is also written to a log file.</remarks>
        /// <param name="category">The category of the log message, used to group related messages.</param>
        /// <param name="name">The name associated with the log message, typically identifying the source or context.</param>
        /// <param name="message">The message content to be logged.</param>
        /// <param name="includeTimeStamp">If <see langword="true"/>, includes the current timestamp in the log message; otherwise, no timestamp is
        /// included. Defaults to <see langword="false"/>.</param>
        public static void Log(string category, string name, string message, bool includeTimeStamp = false)
        {
            if (!DebugMode)
                return; // If DebugMode is disabled do nothing

            string prefix = $"[{category}] [{name}]";

            if (includeTimeStamp)
                prefix = $"[{DateTime.Now:HH:mm:ss}] {prefix}";

            string fullMessage = $"{prefix} {message}";
            System.Diagnostics.Debug.WriteLine(fullMessage);
            if (LogtoFile)
                LogToFile(fullMessage);
        }
        /// <summary>
        /// Logs a message to the debug output and optionally to a file if the specified condition is met.
        /// </summary>
        /// <remarks>The method logs messages only when the application is in debug mode. If <paramref
        /// name="includeTimeStamp"/> is <see langword="true"/>, the current time is prefixed to the log
        /// message.</remarks>
        /// <param name="condition">A boolean value that determines whether the message should be logged. The message is logged only if this
        /// value is <see langword="true"/>.</param>
        /// <param name="category">The category of the log message, used to group related messages.</param>
        /// <param name="name">The name associated with the log message, typically identifying the source or context of the message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="includeTimeStamp">A boolean value indicating whether to include a timestamp in the log message. Defaults to <see
        /// langword="false"/>.</param>
        public static void LogIf(bool condition, string category, string name, string message, bool includeTimeStamp = false)
        {
            if (!DebugMode)
                return; // If DebugMode is disabled do nothing            
            string prefix = $"[{category}] [{name}]";

            if (includeTimeStamp)
                prefix = $"[{DateTime.Now:HH:mm:ss}] {prefix}";

            string fullMessage = $"{prefix} {message}";

            System.Diagnostics.Debug.WriteLineIf(condition, $"{prefix} {message}");
            if (LogtoFile) LogToFile(fullMessage);
        }

        /// <summary>
        /// Logs a message to a file, creating a new log file if necessary and archiving old logs.
        /// </summary>
        /// <remarks>If the log file exceeds the maximum size, it is archived with a timestamp, and older
        /// archives are deleted to maintain a limited number of backups. The log directory is created if it does not
        /// exist.</remarks>
        /// <param name="message">The message to be logged. This message will be appended to the current log file.</param>
        private static void LogToFile(string message)
        {
            //Only do stuff in DebugMode
            if (!DebugMode)
                return;

            try
            {
                if (!System.IO.Directory.Exists("Logs"))
                {
                    System.IO.Directory.CreateDirectory("Logs");
                }
                if (System.IO.File.Exists(LogFile) && new System.IO.FileInfo(LogFile).Length >= MaxSizeBytes)
                {
                    Files.CreateBackup(LogFile);
                }
                System.IO.File.AppendAllText(LogFile, message + Environment.NewLine);

            }
            catch
            { }
        }

    }
}
