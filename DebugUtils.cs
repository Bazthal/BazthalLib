using BazthalLib.Systems.IO;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace BazthalLib
{


    public class DebugUtils
    {
        [EditorBrowsable(EditorBrowsableState.Never)] // Hide from IntelliSense
        public static bool DebugMode { get; set; } = false;
        public static bool LogtoFile { get; set; } = false;
        public static long MaxSizeBytes { get; set; } = 5 * 1024 * 1024;
        public static int MaxBackUps { get; set; } = 5;
        private static readonly string LogFile = Path.Combine(Application.StartupPath, @"Logs/debug.log");            

        /// <summary>
        /// Specifies the severity level of a log message.
        /// </summary>
        /// <remarks>The <see cref="LogLevel"/> enumeration is used to categorize log messages by their
        /// importance or severity. It helps in filtering and managing log output based on the level of detail
        /// required.</remarks>
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Critical,
            Debug,
            Obsolete
        }

        /// <summary>
        /// Specifies the category of log messages.
        /// </summary>
        /// <remarks>This enumeration is used to classify log messages into different categories,  such as
        /// general application events, network operations, user interface actions,  database interactions, performance
        /// metrics, and security-related events.  It helps in filtering and organizing log output for better analysis
        /// and monitoring.</remarks>
        public enum LogCategory
        {
            General,
            Network,
            UI,
            Database,
            Performance,
            Security,
            Configuration,
            Serialization,
            Forms,
            Theme,
            System
        }

        /// <summary>
        /// Specifies the different categories of log entries.
        /// </summary>
        /// <remarks>This enumeration is used to classify log messages into distinct categories, allowing
        /// for easier filtering and analysis of log data.</remarks>
        public enum LogName
        {
            Initialization,
            Connection,
            UserAction,
            Query,
            Update,
            Deletion
        }

        /// <summary>
        /// Initializes static members of the <see cref="DebugUtils"/> class.
        /// </summary>
        /// <remarks>This static constructor ensures that the directory for the log file exists. If the
        /// directory does not exist, it attempts to create it. Any exceptions during directory creation are caught and
        /// ignored.</remarks>
        static DebugUtils()
        {
            try
            {
                string dir = Path.GetDirectoryName(LogFile)!;
                if (DebugMode)
                {
                    Files.CreateDirectory(dir);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Logs a message with a specified category, name, and log level, optionally including a timestamp.
        /// </summary>
        /// <remarks>The method writes the log message to the system diagnostics trace and, if enabled, to
        /// a file.  Logging occurs only when DebugMode is enabled.</remarks>
        /// <param name="category">The category of the log message, used to group related messages.</param>
        /// <param name="name">The name associated with the log message, typically identifying the source or context.</param>
        /// <param name="message">The message content to be logged.</param>
        /// <param name="includeTimeStamp">Indicates whether to include a timestamp in the log message. Defaults to <see langword="true"/>.</param>
        /// <param name="logLevel">The severity level of the log message. Defaults to <see cref="LogLevel.Debug"/>.</param>
        public static void Log(string category, string name, string message,bool includeTimeStamp = true, LogLevel logLevel = LogLevel.Debug)
        {
            if (!DebugMode)
                return; // If DebugMode is disabled do nothing

            string prefix = $"[{logLevel}] [{category}] [{name}]";

            if (includeTimeStamp)
                prefix = $"[{DateTime.Now:HH:mm:ss}] {prefix}";

            string fullMessage = $"{prefix} {message}";

            System.Diagnostics.Trace.WriteLine(fullMessage);
            if (LogtoFile)
                LogToFile(fullMessage);
        }

        /// <summary>
        /// Logs a message with an optional timestamp and specified log level.
        /// </summary>
        /// <remarks>This method logs the provided message under the "General" category with the method
        /// name "Log".</remarks>
        /// <param name="message">The message to log. Cannot be null or empty.</param>
        /// <param name="includeTimeStamp">If <see langword="true"/>, includes a timestamp in the log entry; otherwise, omits the timestamp. Defaults
        /// to <see langword="true"/>.</param>
        /// <param name="logLevel">The level of the log entry, such as Debug, Info, or Error. Defaults to <see cref="LogLevel.Debug"/>.</param>
        public static void Log(string message, bool includeTimeStamp = true, LogLevel logLevel = LogLevel.Debug)
        {
            Log("General", "Log", message, includeTimeStamp, logLevel);
        }

        /// <summary>
        /// Logs a message with the specified category, name, and log level.
        /// </summary>
        /// <param name="category">The category of the log entry, which helps in classifying the log message.</param>
        /// <param name="name">The name associated with the log entry, typically used to identify the source or context of the log message.</param>
        /// <param name="message">The message to be logged, providing details about the event or action being recorded.</param>
        /// <param name="includeTimeStamp">Indicates whether to include a timestamp in the log entry. Defaults to <see langword="true"/>.</param>
        /// <param name="logLevel">The level of the log entry, determining the severity or importance of the log message. Defaults to <see
        /// cref="LogLevel.Debug"/>.</param>
        public static void Log(LogCategory category, LogName name, string message, bool includeTimeStamp = true, LogLevel logLevel = LogLevel.Debug)
        {
            Log(category.ToString(), name.ToString(), message, includeTimeStamp, logLevel);
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
        /// langword="true"/>.</param>
        public static void LogIf(bool condition, string category, string name, string message, bool includeTimeStamp = true, LogLevel logLevel = LogLevel.Debug)
        {
            if (condition)
            {
                Log(category, name, message, includeTimeStamp, logLevel);
            }
        }

        /// <summary>
        /// Logs a message indicating that a specified type is obsolete and has been replaced by a new type.
        /// </summary>
        /// <remarks>This method generates a log entry to inform developers about the deprecation of a
        /// type and provides guidance on updating references to the new type.</remarks>
        /// <param name="oldTypeName">The name of the obsolete type.</param>
        /// <param name="newTypeName">The name of the new type that replaces the obsolete type.</param>
        public static void LogObsoleteUsage(string oldTypeName, string newTypeName)
        {
            Log(oldTypeName, oldTypeName,
                $"The type '{oldTypeName}' is obsolete and has been moved to '{newTypeName}'. Please update your references accordingly.", logLevel: LogLevel.Obsolete);
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
                Files.CreateDirectory("Logs");

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
