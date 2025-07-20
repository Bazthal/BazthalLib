using System.Drawing;

namespace BazthalLib.UI
{
    /// <summary>
    /// Represents a set of colors used for theming an application's user interface.
    /// </summary>
    /// <remarks>The <see cref="ThemeColors"/> class provides properties to define various colors used in a
    /// theme, such as background, foreground, border, and accent colors. These colors can be used to maintain a
    /// consistent look and feel across the application.</remarks>
    public class ThemeColors
    {
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public Color BorderColor { get; set; }
        public Color AccentColor { get; set; }
        public Color SelectedItemBackColor { get; set; }
        public Color SelectedItemForeColor { get; set; }
        public Color DisabledColor { get; set; }
    }
}
