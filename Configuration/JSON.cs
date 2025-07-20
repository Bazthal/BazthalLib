using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BazthalLib.Configuration
{
    public class JSON<T> where T : class, new()
    {
        public T Data { get; private set; }
        public void SetData(T value) => Data = value;
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JSON{T}"/> class with the specified file path and optional JSON
        /// converters.
        /// </summary>
        /// <remarks>The <see cref="JSON{T}"/> class provides functionality to handle JSON data from a
        /// specified file path. The <paramref name="converters"/> parameter allows for custom serialization behavior by
        /// adding user-defined converters to the <see cref="JsonSerializerOptions"/>.</remarks>
        /// <param name="filePath">The path to the JSON file to be processed. This cannot be null or empty.</param>
        /// <param name="converters">An optional array of <see cref="JsonConverter"/> objects to customize the serialization and deserialization
        /// process. If null, no additional converters are used.</param>
        public JSON(string filePath, JsonConverter[]? converters = null)
        {
            _filePath = filePath;
            Data = new T();

            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    _options.Converters.Add(converter);
                }
            }
        }

        /// <summary>
        /// Loads configuration data from a JSON file into the <see cref="Data"/> property.
        /// </summary>
        /// <remarks>If the specified file does not exist, or if an error occurs during loading,  the <see
        /// cref="Data"/> property is initialized with default values.</remarks>
        public void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    Data = JsonSerializer.Deserialize<T>(json, _options) ?? new T();
                }
                else
                {
                    DebugUtils.Log("JSON", "Load", $"Config Not found.. Using default values");
                    Data = new T();
                }
            }
            catch
            {
                DebugUtils.Log("JSON", "Load", $"Config didn't load correctly.. Using default values");
                Data = new T(); // fallback on error
            }
        }
        /// <summary>
        /// Saves the current configuration data to a file in JSON format.
        /// </summary>
        /// <remarks>The configuration data is serialized using the specified JSON options and written to
        /// the file path provided. If an error occurs during the save operation, a debug message is logged.</remarks>
        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Data, _options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                DebugUtils.Log("JSON", "Save", $"Config didn't Save");
            }
        }
    }
}
