using System.Diagnostics;


namespace BazthalLib.Systems.IO
{
    public class Execution
    {
        /// <summary>
        /// Executes an external program specified by the given file path with the provided arguments.
        /// </summary>
        /// <remarks>If the specified file does not exist, the method logs a message indicating that the
        /// file was not found.</remarks>
        /// <param name="path">The full path to the executable file to run. The file must exist at this location.</param>
        /// <param name="args">The command-line arguments to pass to the executable. This can be an empty string if no arguments are
        /// needed.</param>
        public static void RunExecutable(string path, string args)
        {
            if (System.IO.File.Exists(path))
            {
                _ = Process.Start(path, args);
                DebugUtils.Log("Process", "RunExecutable", $"Running Executable: \n  {path} \n with the following arguments: \n {args}");
            }
            else
            {
                DebugUtils.Log("Process", "RunExecutable", $"File not found: {path}");
            }
        }

        /// <summary>
        /// Executes a specified executable file with given arguments in a hidden window and captures its output.
        /// </summary>
        /// <remarks>This method is useful for running command-line tools that require input and capturing
        /// their output without displaying a window.</remarks>
        /// <param name="path">The full path to the executable file to run. The file must exist at this location.</param>
        /// <param name="args">The command-line arguments to pass to the executable.</param>
        /// <returns>The standard output produced by the executable as a string. Returns an empty string if the file does not
        /// exist or if no output is produced.</returns>
        public static string RunHiddenExecutable(string path, string args)
        {
            if (System.IO.File.Exists(path))
            {
                //Mainly to be used with RCON or other command line tools that require input x
                System.Diagnostics.Process run = new();
                run.StartInfo.UseShellExecute = false;
                run.StartInfo.CreateNoWindow = true;
                run.StartInfo.FileName = path;
                run.StartInfo.Arguments = args;
                run.StartInfo.RedirectStandardOutput = true;
                run.StartInfo.RedirectStandardInput = true;
                run.Start();
                string q = "";
                while (!run.HasExited)
                {
                    q += run.StandardOutput.ReadToEnd();
                }
                DebugUtils.Log("Process", "RunHiddenExecutable", $"Running Executable: \n  {path} \n with the following arguments: \n {args}");

                return q;
            }
            else
            {
                DebugUtils.Log("Process", "RunHiddenExecutable", $"File not found: {path}");
            }
            return "";
        }
    }
}
