using System;

namespace BazthalLib.Events
{
    /// <summary>
    /// Provides data for an event that occurs when files are dropped.
    /// </summary>
    /// <remarks>This event argument contains the file paths of the dropped files. The <see cref="Files"/>
    /// property will always contain a non-null array, which may be empty if no files were provided.</remarks>
    public class FilesDroppedEventArgs : EventArgs
    {
        public string[] Files { get; }

        /// <summary>
        /// Provides data for the event that occurs when files are dropped.
        /// </summary>
        /// <param name="files">An array of file paths representing the files that were dropped. If no files are provided, an empty array is
        /// used.</param>
        public FilesDroppedEventArgs(string[] files)
        {
            Files = files ?? Array.Empty<string>();
        }
    }

}
