using BazthalLib.UI;

namespace BazthalLib.Controls
{
    /// <summary>
    /// Represents a control that can have a theme applied to it.
    /// </summary>
    public interface IThemableControl
    {
        /// <summary>
        /// Applies the specified theme colors to the application interface.
        /// </summary>
        /// <remarks>The method updates the application's visual elements to reflect the new theme colors.
        /// Ensure that the <paramref name="colors"/> parameter is not null to avoid exceptions.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the color settings to apply.</param>
        void ApplyTheme(ThemeColors colors);
    }
}
