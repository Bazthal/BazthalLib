using System.Collections.Generic;
using System.IO;

namespace BazthalLib.Configuration
{
    public class INI
    {
        /// <summary>
        /// Loads configuration data from an INI file into a nested dictionary structure.
        /// </summary>
        /// <remarks>The method reads the specified INI file line by line, identifying sections and
        /// key-value pairs. Sections are denoted by lines enclosed in square brackets (e.g., [SectionName]), and
        /// key-value pairs are denoted by lines containing an equals sign (e.g., key=value). Lines outside of sections
        /// or without an equals sign are ignored. If the file does not exist, the method logs a message and returns an
        /// empty dictionary.</remarks>
        /// <param name="filePath">The path to the INI file to be loaded. Must not be null or empty.</param>
        /// <returns>A dictionary where each key is a section name from the INI file, and the value is another dictionary
        /// containing key-value pairs of configuration settings within that section. Returns an empty dictionary if the
        /// file does not exist or contains no valid sections.</returns>
        public static Dictionary<string, Dictionary<string, string>> Load(string filePath)
        {
            var configData = new Dictionary<string, Dictionary<string, string>>();

            if (!System.IO.File.Exists(filePath))
            {
                DebugUtils.Log("Configuration", "INI Load", "Config file not found.");
                return configData;
            }

            string currentSection = "";
            foreach (var line in System.IO.File.ReadAllLines(filePath))
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Trim('[', ']');
                    if (!configData.ContainsKey(currentSection))
                        configData[currentSection] = new Dictionary<string, string>();
                }
                else if (trimmedLine.Contains("=") && !string.IsNullOrWhiteSpace(currentSection))
                {
                    string[] parts = trimmedLine.Split(new[] { '=' }, 2);
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    configData[currentSection][key] = value;
                }
            }

            return configData;
        }

        /// <summary>
        /// Edits an INI configuration file by applying specified changes to its sections and keys.
        /// </summary>
        /// <remarks>If the specified file does not exist, a new configuration file will be created with
        /// the provided changes. Existing keys in the file will be updated with new values if specified in the changes
        /// dictionary. New keys will be added under their respective sections if they do not already exist.</remarks>
        /// <param name="filePath">The path to the INI file to be edited. If the file does not exist, a new file will be created.</param>
        /// <param name="changes">A dictionary containing the sections and their corresponding key-value pairs to be modified or added. The
        /// outer dictionary's keys represent section names, and the inner dictionary's keys and values represent the
        /// keys and their new values within those sections.</param>
        public static void Edit(string filePath, Dictionary<string, Dictionary<string, string>> changes)
        {
            var lines = new List<string>();
            string currentSection = "";
            bool changesMade = false;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            if (System.IO.File.Exists(filePath))
            {
                lines.AddRange(System.IO.File.ReadAllLines(filePath));
            }
            else
            {
                DebugUtils.Log("Configuration", "INI Edit", "Config file not found. Creating new config file.");
                foreach (var section in changes)
                {
                    lines.Add("[" + section.Key + "]");
                    foreach (var kvp in section.Value)
                    {
                        lines.Add(kvp.Key + "=" + kvp.Value);
                    }
                    lines.Add(""); // Add spacing between sections
                }
                System.IO.File.WriteAllLines(filePath, lines);
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // Detect section headers
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Trim('[', ']'); // Store the section name
                    continue;
                }

                // Modify settings in relevant sections
                if (changes.ContainsKey(currentSection) && line.Contains("="))
                {
                    string[] parts = line.Split(new[] { '=' }, 2);
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (changes[currentSection].ContainsKey(key))
                    {
                        lines[i] = key + "=" + changes[currentSection][key]; // Remove whitespace
                        changes[currentSection].Remove(key); // Prevent duplicate insertions
                        changesMade = true;
                    }
                }
            }

            // Append missing keys under their respective sections
            if (changes.Count > 0)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                    {
                        currentSection = lines[i].Trim('[', ']'); // Update current section
                    }

                    if (changes.ContainsKey(currentSection) && changes[currentSection].Count > 0)
                    {
                        foreach (var kvp in changes[currentSection])
                        {
                            lines.Insert(i + 1, kvp.Key + "=" + kvp.Value); // Remove whitespace
                        }
                        changes[currentSection].Clear(); // Ensure keys are not added multiple times
                        changesMade = true;
                    }
                }
            }

            // Save changes if any were made
            if (changesMade)
            {
                System.IO.File.WriteAllLines(filePath, lines);
            }
        }


    }
}
