using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BazthalLib.Controls;

namespace BazthalLib.UI
{
    public class Theming
    {
        private static class AutoRegisterSystem
        {
            private static readonly List<Control> _themableControls = new();
            /// <summary>
            /// Registers a control and its child controls for theming support.
            /// </summary>
            /// <remarks>This method adds the specified control and its child controls to a collection
            /// of themable controls, applying the current theme to each. It also hooks into events to manage controls
            /// added or removed dynamically.</remarks>
            /// <param name="control">The control to register. Must implement <see cref="IThemableControl"/> to be themed.</param>
            public static void Register(Control control)
            {
                if (control is IThemableControl)
                {
                    _themableControls.Add(control);
                    ApplyControlTheme(control, CurrentTheme);
                }

                // Also register child controls recursively
                foreach (Control child in control.Controls)
                {
                    Register(child);
                }

                // Hook into dynamically added controls
                control.ControlAdded += (s, e) => Register(e.Control);
                control.ControlRemoved += (s, e) => _themableControls.Remove(e.Control);
            }

            /// <summary>
            /// Applies the specified theme to all themable controls within the application.
            /// </summary>
            /// <remarks>This method iterates over all controls that implement the <see
            /// cref="IThemableControl"/> interface and applies the given theme to each. Ensure that the <paramref
            /// name="theme"/> parameter is not null before calling this method to avoid unexpected behavior.</remarks>
            /// <param name="theme">The theme to be applied to each control. Cannot be null.</param>
            public static void ApplyThemeToAll(AppTheme theme)
            {
                foreach (var control in _themableControls)
                {
                    if (control is IThemableControl themable)
                    {
                        ApplyControlTheme(control, theme);
                    }
                }
            }
            /// <summary>
            /// Applies the specified theme to a control that supports theming.
            /// </summary>
            /// <remarks>This method updates the control's appearance based on the provided theme. The
            /// control is invalidated after the theme is applied to ensure it is redrawn with the new
            /// settings.</remarks>
            /// <param name="control">The control to which the theme will be applied. Must implement <see cref="IThemableControl"/>.</param>
            /// <param name="theme">The theme to apply to the control.</param>
            private static void ApplyControlTheme(Control control, AppTheme theme)
            {
                if (control is IThemableControl themableControl)
                {
                    var themeColors = GetThemeColors();
                    themableControl.ApplyTheme(themeColors);
                    control.Invalidate();
                }
            }
        }

/// <summary>
/// Specifies the available themes for an application.
/// </summary>
/// <remarks>The <see cref="AppTheme"/> enumeration provides options for setting the visual theme of an
/// application.</remarks>
        public enum AppTheme
        {
            System,
            Dark,
            Light,
            Custom
        }


        private static readonly List<Form> RegisteredForms = [];
        private static AppTheme? _currentTheme = null;
        /// <summary>
        /// Gets the current application theme.
        /// </summary>
        public static AppTheme CurrentTheme
        {
            get
            {
                if (_currentTheme == null)
                {
                    // Fallback to system theme unless explicitly set
                    SetTheme(AppTheme.System);
                }
                return _currentTheme.Value;
            }
        }
        /// <summary>
        /// Registers a form for theme application and automatic system registration.
        /// </summary>
        /// <remarks>Once registered, the form will have the current theme applied and will be
        /// automatically registered with the system. The form is automatically unregistered when it is
        /// closed.</remarks>
        /// <param name="form">The form to be registered. Cannot be null.</param>
        public static void RegisterForm(Form form)
        {
            if (!RegisteredForms.Contains(form))
            {
                RegisteredForms.Add(form);
                ApplyTheme(form, CurrentTheme);

                AutoRegisterSystem.Register(form);

                form.FormClosed += (s, e) => RegisteredForms.Remove(form);
            }
        }
        /// <summary>
        /// Sets the custom theme colors for the application.
        /// </summary>
        /// <param name="theme">The <see cref="ThemeColors"/> object representing the custom theme to be applied.</param>
        public static void SetCustomTheme(ThemeColors theme)
        {
            CustomThemeColors = theme;
        }

        /// <summary>
        /// Sets the application's theme to the specified <see cref="AppTheme"/>.
        /// </summary>
        /// <remarks>This method updates the theme for all registered forms and applies the specified
        /// theme globally using the <see cref="AutoRegisterSystem"/>.</remarks>
        /// <param name="theme">The theme to apply. If <see cref="AppTheme.Custom"/> is specified and custom colors are not set, the theme
        /// defaults to <see cref="AppTheme.System"/>.</param>
        public static void SetTheme(AppTheme theme)
        {
            DebugUtils.Log("Theming", "SetTheme", $"Setting theme to {theme}");

            // If custom is selected but colors not set, fallback to System
            if (theme == AppTheme.Custom && CustomThemeColors == null)
            {
                theme = AppTheme.System;
            }

            _currentTheme = theme;

            foreach (var form in RegisteredForms)
                ApplyTheme(form, theme);


            _currentTheme = theme;
            foreach (var form in RegisteredForms)
            {
                ApplyTheme(form, theme);
            }
            AutoRegisterSystem.ApplyThemeToAll(theme);
        }

        /// <summary>
        /// Retrieves the current theme colors based on the application's theme settings.
        /// </summary>
        /// <remarks>This method returns different color configurations depending on the current
        /// application theme: <list type="bullet"> <item> <description>For <see cref="AppTheme.Dark"/>, it returns a
        /// set of colors suitable for a dark theme.</description> </item> <item> <description>For <see
        /// cref="AppTheme.Light"/>, it returns a set of colors suitable for a light theme.</description> </item> <item>
        /// <description>For <see cref="AppTheme.Custom"/>, it returns the custom theme colors if available; otherwise,
        /// it returns an empty <see cref="ThemeColors"/> object.</description> </item> <item> <description>For any
        /// other theme, it returns a default set of system colors.</description> </item> </list></remarks>
        /// <returns>A <see cref="ThemeColors"/> object containing the color settings for the current theme. The colors vary
        /// depending on whether the theme is dark, light, custom, or default.</returns>
        public static ThemeColors GetThemeColors()
        {
            return CurrentTheme switch
            {
                AppTheme.Dark => new ThemeColors
                {
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    BorderColor = Color.DarkGray,
                    AccentColor = Color.DarkGray,
                    SelectedItemBackColor = Color.FromArgb(50, 50, 50),
                    SelectedItemForeColor = Color.White,
                    DisabledColor = Color.FromArgb(80, 80, 80)
                },
                AppTheme.Light => new ThemeColors
                {
                    BackColor = Color.WhiteSmoke,
                    ForeColor = Color.Black,
                    BorderColor = Color.DarkGray,
                    AccentColor = Color.DarkGray,
                    SelectedItemBackColor = Color.LightGray,
                    SelectedItemForeColor = Color.Black,
                    DisabledColor = Color.FromArgb(200, 200, 200)
                },
                AppTheme.Custom => CustomThemeColors ?? new ThemeColors // use an empty fallback
                { },
                _ => new ThemeColors
                {
                    BackColor = SystemColors.Control,
                    ForeColor = SystemColors.ControlText,
                    BorderColor = SystemColors.ActiveBorder,
                    AccentColor = Color.DodgerBlue,
                    SelectedItemBackColor = SystemColors.Highlight,
                    SelectedItemForeColor = SystemColors.HighlightText,
                    DisabledColor = SystemColors.ControlDark
                }
            };
        }

        /// <summary>
        /// Gets the custom theme colors used for UI elements.
        /// </summary>
        public static ThemeColors CustomThemeColors { get; private set; } = new ThemeColors
        {
            BackColor = SystemColors.Control,
            ForeColor = SystemColors.ControlText,
            BorderColor = SystemColors.ActiveBorder,
            AccentColor = Color.DodgerBlue,
            SelectedItemBackColor = Color.Aqua,
            SelectedItemForeColor = Color.Blue,
            DisabledColor = SystemColors.ControlDark
        };

        /// <summary>
        /// Converts a color string representation to its known color name, if available.
        /// </summary>
        /// <remarks>If the input string is in the format "A=alpha, R=red, G=green, B=blue", the method
        /// attempts to match the ARGB values to a known color. If no match is found, it returns the ARGB values as a
        /// hex string. If the input is already a known color name, it is returned unchanged.</remarks>
        /// <param name="colorString">A string representing a color, either in the format "A=alpha, R=red, G=green, B=blue" or as a known color
        /// name.</param>
        /// <returns>The name of the known color if the ARGB values match a non-system known color; otherwise, the original
        /// string or an ARGB hex representation.</returns>
        public static string GetColorNameFromString(string colorString)
        {
            //Add Support for string like this "Color [White]" and just pass directly through
            var match = Regex.Match(colorString, @"A=(\d+), R=(\d+), G=(\d+), B=(\d+)");
            if (!match.Success)
                return colorString;

            int a = int.Parse(match.Groups[1].Value);
            int r = int.Parse(match.Groups[2].Value);
            int g = int.Parse(match.Groups[3].Value);
            int b = int.Parse(match.Groups[4].Value);

            Color inputColor = Color.FromArgb(a, r, g, b);

            // Check only non-system known colors
            foreach (KnownColor known in Enum.GetValues(typeof(KnownColor)))
            {
                Color knownColor = Color.FromKnownColor(known);
                if (!knownColor.IsSystemColor && knownColor.ToArgb() == inputColor.ToArgb())
                    return knownColor.Name;
            }

            // Fallback to ARGB hex if no known color match
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Applies the specified theme to the given control and all its child controls.
        /// </summary>
        /// <remarks>If the control implements <see cref="IThemableControl"/>, the theme is applied using
        /// the control's custom theming logic. Otherwise, the control's background and foreground colors are set
        /// directly. The method recursively applies the theme to all child controls.</remarks>
        /// <param name="control">The root control to which the theme will be applied. This control and all its descendants will be themed.</param>
        /// <param name="theme">The theme to apply to the control. This determines the color scheme used.</param>
        public static void ApplyTheme(Control control, AppTheme theme)
        {
            var colors = GetThemeColors();

            if (control is IThemableControl themable)
            {
                themable.ApplyTheme(colors);
            }
            else
            {
                control.BackColor = colors.BackColor;
                control.ForeColor = colors.ForeColor;
            }

            foreach (Control child in control.Controls)
            {
                ApplyTheme(child, theme);
            }

            control.Invalidate();
        }
    }
}
