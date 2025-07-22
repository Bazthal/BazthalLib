using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace BazthalLib.Systems.IO
{

    public class Files
    {
        /// <summary>
        /// Deletes the specified file if it exists.
        /// </summary>
        /// <remarks>This method does nothing if the file specified by <paramref name="path"/> does not
        /// exist.</remarks>
        /// <param name="path">The path to the file to be deleted. Must not be null or empty.</param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes the specified directory and its contents.
        /// </summary>
        /// <remarks>If the directory specified by <paramref name="path"/> exists, it and all its contents
        /// are deleted.</remarks>
        /// <param name="path">The path to the directory to be deleted. Must not be null or empty.</param>
        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Creates a new directory at the specified path if it does not already exist.
        /// </summary>
        /// <remarks>If the directory already exists, no action is taken. Ensure that the application has
        /// the necessary permissions to create directories at the specified path.</remarks>
        /// <param name="path">The path where the directory should be created. Cannot be null or empty.</param>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Extracts the contents of a zip file to a specified directory.
        /// </summary>
        /// <remarks>This method opens the specified zip file, extracts all its contents to the given
        /// destination directory, and optionally deletes the zip file based on the <paramref name="deleteZip"/>
        /// parameter.</remarks>
        /// <param name="path">The file path of the zip archive to extract. Cannot be null or empty.</param>
        /// <param name="destination">The directory path where the contents will be extracted. Must be a valid directory path.</param>
        /// <param name="deleteZip">If <see langword="true"/>, deletes the zip file after extraction; otherwise, the zip file is retained.</param>
        public static void UnzipFile(string path, string destination, bool deleteZip)
        {
            using FileStream zipFile = File.Open(path, FileMode.Open);
            using var archive = new ZipArchive(zipFile);
                archive.ExtractToDirectory(destination);
                archive.Dispose();
                if (deleteZip == true)
                {
                    DeleteFile(path);
                }            
        }
        /// <summary>
        /// Compresses the contents of the specified directory into a zip archive.
        /// </summary>
        /// <remarks>The method creates a zip archive at the specified destination containing all files
        /// and subdirectories from the specified directory. If the destination file already exists, it will be
        /// overwritten.</remarks>
        /// <param name="path">The path to the directory whose contents are to be compressed. Must be a valid directory path.</param>
        /// <param name="destination">The path where the resulting zip archive will be saved. Must be a valid file path.</param>
        public static void CompressFile(string path, string destination)
        {
            ZipFile.CreateFromDirectory(path, destination);
        }

        /// <summary>
        /// Displays a file selection dialog and returns the selected file path.
        /// </summary>
        /// <remarks>The method uses an <see cref="OpenFileDialog"/> to allow the user to select a file.
        /// If <paramref name="multiselect"/> is set to <see langword="true"/>, only the first selected file's path is
        /// returned.</remarks>
        /// <param name="filename">The initial file name to display in the dialog. Can be an empty string.</param>
        /// <param name="filter">The filter string that determines the types of files to display. Can be an empty string to show all files.</param>
        /// <param name="title">The title of the dialog window. Defaults to "Select a file".</param>
        /// <param name="multiselect">A value indicating whether multiple files can be selected. Defaults to <see langword="false"/>.</param>
        /// <returns>The full path of the selected file if a file is chosen; otherwise, <see langword="null"/>.</returns>
        public static string ChooseFile(string filename = "", string filter = "", string title = "Select a file", bool multiselect = false)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = filter,
                FileName = filename,
                Title = title,
                Multiselect = multiselect
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Displays a file selection dialog that allows the user to choose multiple files.
        /// </summary>
        /// <param name="filename">The initial file name to display in the dialog. This can be an empty string if no initial file name is
        /// needed.</param>
        /// <param name="filter">The filter string that determines the types of files to display. This is a semicolon-separated list of file
        /// type filters, such as "Text files (*.txt)|*.txt|All files (*.*)|*.*".</param>
        /// <param name="title">The title of the dialog window. Defaults to "Select files".</param>
        /// <returns>An array of strings containing the full paths of the selected files. Returns <see langword="null"/> if the
        /// dialog is canceled or no files are selected.</returns>
        public static string[] ChooseFiles(string filename = "", string filter = "", string title = "Select files")
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = filter,
                FileName = filename,
                Title = title,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileNames;
            }
            return null;
        }

        /// <summary>
        /// Displays a dialog that allows the user to select a folder.
        /// </summary>
        /// <remarks>This method uses a <see cref="FolderBrowserDialog"/> to prompt the user for a folder
        /// selection.</remarks>
        /// <param name="title">The description text displayed above the tree view control in the dialog box.</param>
        /// <param name="newFolder">A value indicating whether the dialog box includes a button to create a new folder.</param>
        /// <param name="setTitle">A value indicating whether the description text is used as the dialog box title.</param>
        /// <param name="initialPath">The initial path selected when the dialog box is displayed. Defaults to an empty string.</param>
        /// <returns>The path of the selected folder if the user clicks OK; otherwise, <see langword="null"/>.</returns>
        public static string ChooseFolder(string title, bool newFolder, bool setTitle, string initialPath = "")
        {
            FolderBrowserDialog folderBrowserDialog = new()
            {
                SelectedPath = initialPath,
                Description = title,
                ShowNewFolderButton = newFolder,
                UseDescriptionForTitle = setTitle
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }
            return null;
        }

        /// <summary>
        /// Displays a save file dialog and returns the selected file path.
        /// </summary>
        /// <param name="filename">The default file name to display in the dialog. Can be an empty string.</param>
        /// <param name="filter">The filter string that determines the choices that appear in the "Save as type" box. Can be an empty string.</param>
        /// <param name="title">The title of the save file dialog. Defaults to "Save File".</param>
        /// <returns>The full path of the file selected by the user, or <see langword="null"/> if the dialog is canceled.</returns>
        public static string SaveFile(string filename = "", string filter = "", string title = "Save File")
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = filter,
                FileName = filename,
                Title = title
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Creates a backup of the specified file and manages the number of backup files.
        /// </summary>
        /// <remarks>The method moves the original file to a new location with a timestamp appended to its
        /// name, effectively creating a backup. It then ensures that only the specified number of recent backups are
        /// retained, deleting older ones.</remarks>
        /// <param name="origFile">The path of the original file to back up. This parameter cannot be null or empty.</param>
        /// <param name="maxBackups">The maximum number of backup files to retain. Older backups beyond this limit will be deleted. Must be a
        /// non-negative integer. The default is 5.</param>

        public static void CreateBackup(string origFile, int maxBackups = 5)
        {
            string folder = Path.GetDirectoryName(origFile) ?? ".";
            string fileName = Path.GetFileNameWithoutExtension(origFile);
            string extention = Path.GetExtension(origFile);
                        
            string dest = Path.Combine(folder, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}{extention}");

            DebugUtils.Log("Backup", "Setup", $"Orignal File:{origFile}\nFolder: {folder}\nFileName: {fileName}\nExtention: {extention}\nDestination: {dest}");
            try { System.IO.File.Move(origFile, dest); }
            catch (Exception ex) { DebugUtils.Log("Backup", "Backup Old", $"Unable to backup old file - {ex.Message}");   }

            var backups = System.IO.Directory.GetFiles(folder, $"{fileName}_*.{extention}")
                .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                .Skip(maxBackups);


            foreach (var old in backups)
                try { System.IO.File.Delete(old); }
                catch (Exception ex ){ DebugUtils.Log("Backup", "Prune Old", $"Unable to delete old file {ex.Message}"); }

            
        }
    }

}
