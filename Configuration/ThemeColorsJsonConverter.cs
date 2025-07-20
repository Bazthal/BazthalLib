using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BazthalLib.UI;

namespace BazthalLib.Configuration
{
   
    public class ThemeColorsJsonConverter : JsonConverter<ThemeColors>
    {
        private string _version = "V1.0";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ThemeColors Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var theme = new ThemeColors();

            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                theme.BackColor = TryParseColor(root, "BackColor", SystemColors.Control);
                theme.ForeColor = TryParseColor(root, "ForeColor", SystemColors.ControlText);
                theme.BorderColor = TryParseColor(root, "BorderColor", SystemColors.ActiveBorder);
                theme.AccentColor = TryParseColor(root, "AccentColor", Color.DodgerBlue);
                theme.SelectedItemBackColor = TryParseColor(root, "SelectedItemBackColor", SystemColors.Highlight);
                theme.SelectedItemForeColor = TryParseColor(root, "SelectedItemForeColor", SystemColors.HighlightText);
                theme.DisabledColor = TryParseColor(root, "DisabledColor", SystemColors.ControlDark);
            }

            return theme;
        }
        /// <summary>
        /// Attempts to parse a color from a JSON element by its property name, returning a fallback color if parsing
        /// fails.
        /// </summary>
        /// <remarks>This method supports parsing colors specified as hexadecimal strings (e.g., "#RRGGBB"
        /// or "#AARRGGBB") or as named colors. If the property is not found, is empty, or cannot be parsed, the
        /// fallback color is returned.</remarks>
        /// <param name="root">The JSON element containing the color property.</param>
        /// <param name="name">The name of the property to parse as a color.</param>
        /// <param name="fallback">The color to return if parsing is unsuccessful.</param>
        /// <returns>The parsed <see cref="Color"/> if successful; otherwise, the specified fallback color.</returns>
        private Color TryParseColor(JsonElement root, string name, Color fallback)
        {
            if (!root.TryGetProperty(name, out var prop))
                return fallback;

            try
            {
                string str = prop.GetString();
                if (string.IsNullOrWhiteSpace(str)) return fallback;

                if (str.StartsWith("#"))
                {
                    int argb = int.Parse(str.Substring(1), System.Globalization.NumberStyles.HexNumber);
                    Color parsed = Color.FromArgb(argb);
                    if (parsed.A == 0 && parsed.R == 0 && parsed.G == 0 && parsed.B == 0)
                        return fallback;
                    return parsed;
                }

                Color named = Color.FromName(str);
                return (!named.IsKnownColor && named.ToArgb() == 0) ? fallback : named;
            }
            catch
            {
                return fallback;
            }
        }
        /// <summary>
        /// Writes the JSON representation of a <see cref="ThemeColors"/> object.
        /// </summary>
        /// <remarks>This method serializes the <see cref="ThemeColors"/> object into a JSON object with
        /// properties for each color component.</remarks>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the JSON will be written. This cannot be null.</param>
        /// <param name="value">The <see cref="ThemeColors"/> instance to serialize. This cannot be null.</param>
        /// <param name="options">The serialization options to use. This parameter is not used in the current implementation.</param>
        public override void Write(Utf8JsonWriter writer, ThemeColors value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("BackColor", SerializeColor(value.BackColor));
            writer.WriteString("ForeColor", SerializeColor(value.ForeColor));
            writer.WriteString("BorderColor", SerializeColor(value.BorderColor));
            writer.WriteString("AccentColor", SerializeColor(value.AccentColor));
            writer.WriteString("SelectedItemBackColor", SerializeColor(value.SelectedItemBackColor));
            writer.WriteString("SelectedItemForeColor", SerializeColor(value.SelectedItemForeColor));
            writer.WriteString("DisabledColor", SerializeColor(value.DisabledColor));
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes a <see cref="Color"/> object to a string representation.
        /// </summary>
        /// <param name="value">The <see cref="Color"/> to serialize.</param>
        /// <returns>A string representing the color. If the color is a known color and not a system color,  the method returns
        /// the color's name. Otherwise, it returns the ARGB value in hexadecimal format.</returns>
        private string SerializeColor(Color value)
        {
            return value.IsKnownColor && !value.IsSystemColor
                ? value.Name
                : $"#{value.ToArgb():X8}";
        }
    }

}
