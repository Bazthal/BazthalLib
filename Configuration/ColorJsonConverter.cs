using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BazthalLib.Configuration
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Reads and converts a JSON string representation of a color to a <see cref="Color"/> object.
        /// </summary>
        /// <remarks>The method supports color strings in the formats "#AARRGGBB", "#RRGGBB", and named
        /// colors.  If the color string is invalid or results in a transparent or empty color, a fallback color is
        /// returned.</remarks>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The type of the object to convert, which is expected to be <see cref="Color"/>.</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>A <see cref="Color"/> object that represents the color specified in the JSON string.  Returns <see
        /// cref="Color.Black"/> if the input string is null or whitespace, or <see cref="SystemColors.Control"/> if
        /// parsing fails.</returns>
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            DebugUtils.Log("ColorJsonConverter", "Read", $"Input: {str}");

            if (string.IsNullOrWhiteSpace(str))
            {
                DebugUtils.Log("ColorJsonConverter", "Warning", "Color string was empty or null.");
                return Color.Black; // fallback
            }

            try
            {
                Color parsed;

                if (str.StartsWith('#'))
                {
                    if (str.Length == 9) // #AARRGGBB (preferred)
                    {
                        int argb = int.Parse(str[1..], System.Globalization.NumberStyles.HexNumber);
                        parsed = Color.FromArgb(argb);
                    }
                    else if (str.Length == 7) // #RRGGBB
                    {
                        parsed = ColorTranslator.FromHtml(str);
                    }
                    else
                    {
                        throw new FormatException("Invalid hex format.");
                    }
                }
                else
                {
                    parsed = Color.FromName(str);
                }

                if (parsed.A == 0 || parsed.IsEmpty)
                {
                    throw new ArgumentException("Parsed color is transparent or empty.");
                }

                return parsed;
            }
            catch (Exception ex)
            {
                DebugUtils.Log("ColorJsonConverter", "Error", $"Failed to parse color \"{str}\" — {ex.Message}. Using fallback.");
                return SystemColors.Control; // fallback
            }
        }
        /// <summary>
        /// Writes a JSON string representation of a <see cref="Color"/> value.
        /// </summary>
        /// <remarks>If the <paramref name="value"/> is a known color and not a system color, the method
        /// writes the color's name. Otherwise, it writes the ARGB value of the color in hexadecimal format.</remarks>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the JSON string is written.</param>
        /// <param name="value">The <see cref="Color"/> value to convert to a JSON string.</param>
        /// <param name="options">The serialization options to use. This parameter is not used in this method.</param>
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            if (value.IsKnownColor && !value.IsSystemColor)
            {
                writer.WriteStringValue(value.Name);
            }
            else
            {
                writer.WriteStringValue($"#{value.ToArgb():X8}");
            }
        }
    }

}
