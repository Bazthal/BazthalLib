using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BazthalLib.Extensibility.Serialization
{
    /// <summary>
    /// Provides a custom JSON converter for strings that intelligently handles inline JSON content.
    /// </summary>
    /// <remarks>This converter processes string values to determine if they represent valid JSON content. If
    /// the string appears to be JSON (e.g., starts and ends with curly braces or square brackets), it attempts to parse
    /// and reformat the content. If the JSON is valid, it is written in a compact or indented format depending on its
    /// size and structure. Otherwise, the string is written as-is. <para> Strings that are null, empty, or consist only
    /// of whitespace are serialized as JSON null values. </para> <para> This converter is useful for scenarios where
    /// strings may contain embedded JSON and you want to ensure proper formatting or validation during serialization.
    /// </para></remarks>
    public sealed class InlineJsonConverter : JsonConverter<string>
    {
        private const int MaxInlineLength = 100; 

        /// <summary>
        /// Reads and converts the JSON value to a string.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> positioned at the JSON value to convert.</param>
        /// <param name="typeToConvert">The type of the object to convert. This parameter is not used in this implementation.</param>
        /// <param name="options">The serialization options to use. This parameter is not used in this implementation.</param>
        /// <returns>The string representation of the JSON value, or <see langword="null"/> if the value is <see
        /// langword="null"/>.</returns>
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString();

        /// <summary>
        /// Writes a JSON value to the specified <see cref="Utf8JsonWriter"/> based on the provided string input.
        /// </summary>
        /// <remarks>If the input string resembles JSON but is invalid or cannot be parsed, it will be
        /// written as a JSON string value instead.</remarks>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the JSON value will be written. Cannot be <c>null</c>.</param>
        /// <param name="value">The string to be written. If the string is <c>null</c>, empty, or consists only of whitespace, a JSON null
        /// value is written. If the string appears to be valid JSON (e.g., starts and ends with curly braces or square
        /// brackets), it is parsed and written as raw JSON. Otherwise, the string is written as a JSON string value.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use during serialization. Cannot be <c>null</c>.</param>
        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                writer.WriteNullValue();
                return;
            }

            string trimmed = value.Trim();

            bool looksLikeJson =
                (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                (trimmed.StartsWith("[") && trimmed.EndsWith("]"));

            if (!looksLikeJson)
            {
                writer.WriteStringValue(value);
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                bool shouldIndentPrint = ShouldIndentPrint(doc.RootElement, trimmed);

                var buffer = new ArrayBufferWriter<byte>();
                using var tempWriter = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = shouldIndentPrint });
                doc.WriteTo(tempWriter);

                tempWriter.Flush();
                string jsonText = Encoding.UTF8.GetString(buffer.WrittenSpan);
                writer.WriteRawValue(jsonText);

                buffer.Clear();
                tempWriter.Dispose();
            }
            catch
            {
                writer.WriteStringValue(value);
            }
        }

        /// <summary>
        /// Determines whether the JSON element should be printed with indentation based on its content and structure.
        /// </summary>
        /// <remarks>The decision is based on the length of the raw JSON string and the depth of the JSON
        /// structure. If the raw string exceeds a predefined maximum inline length or the JSON structure has a depth
        /// greater than 1,  the method returns <see langword="true"/>.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> representing the JSON data to evaluate.</param>
        /// <param name="raw">The raw JSON string representation of the element.</param>
        /// <returns><see langword="true"/> if the JSON element should be printed with indentation; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool ShouldIndentPrint(JsonElement element, string raw)
        {
            if (raw.Length > MaxInlineLength)
                return true;

            int maxDepth = GetJsonDepth(element);
            return maxDepth > 1;
        }

        /// <summary>
        /// Calculates the maximum depth of a JSON element.
        /// </summary>
        /// <remarks>The depth of a JSON structure is defined as the maximum level of nesting within the
        /// element.  For example, a JSON object with nested objects or arrays will have a depth equal to the deepest
        /// level of nesting.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to analyze. Must be a valid JSON object, array, or value.</param>
        /// <returns>The maximum depth of the JSON structure. Returns 0 for primitive values or empty elements.</returns>
        private static int GetJsonDepth(JsonElement element)
        {
            int maxDepth = 0;

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                        maxDepth = Math.Max(maxDepth, GetJsonDepth(prop.Value) + 1);
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                        maxDepth = Math.Max(maxDepth, GetJsonDepth(item) + 1);
                    break;

                default:
                    return 0;
            }

            return maxDepth;
        }
    }
}
