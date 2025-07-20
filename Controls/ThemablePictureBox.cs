using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{

    public class ThemablePictureBox : PictureBox, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";

        private Color _borderColor = Color.Gray;
        private Color _accentColor = Color.DodgerBlue;
        private bool _useThemeColors = true;
        private bool _useAccentColor = false;
        private bool _enableBorder = true;
        private ThemeColors _themeColors = new();

        /// <summary>
        /// Gets or sets a value indicating whether theme colors are used for the appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use theme colors or not.")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets the unique identifier for the ThemablePictureBox control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemablePictureBox {_version}";

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the border.")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the accent color used when the control is focused.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The accent color when focused.")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the accent color is used.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use the accent color or not.")]
        [DefaultValue(false)]
        public bool UseAccentColor
        {
            get => _useAccentColor;
            set { _useAccentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to enable the border or not.")]
        [DefaultValue(true)]
        public bool EnableBorder
        {
            get => _enableBorder;
            set { _enableBorder = value; Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get => base.BorderStyle;
            set => base.BorderStyle = value;
        }


        #endregion Fields and Properties


        #region Methods and Events
        /// <summary>
        /// Processes Windows messages and optionally draws a custom border.
        /// </summary>
        /// <remarks>This method overrides the base <c>WndProc</c> to handle specific Windows messages. If
        /// the message is a paint message (0xF) or a non-client area paint message (0x85), and custom borders are
        /// enabled, it draws a custom border.</remarks>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0xF || m.Msg == 0x85)
            {
                if (_enableBorder)
                {
                    DrawCustomBorder();
                }
            }

        }
        /// <summary>
        /// Draws a custom border around the control using the specified border color.
        /// </summary>
        /// <remarks>This method uses the accent color if <see cref="_useAccentColor"/> is <see
        /// langword="true"/>; otherwise, it uses the default border color. The border is drawn only if the control's
        /// handle is created.</remarks>
        private void DrawCustomBorder()
        {
            if (!IsHandleCreated) return;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using Pen pen = new Pen(_useAccentColor ? _accentColor : _borderColor, 1);
                g.DrawRectangle(pen, rect);
            }
        }
        #endregion Methods and Events


        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's background, foreground, border, and accent colors
        /// based on the provided theme. If theming is disabled or the <paramref name="colors"/> parameter is null, the
        /// method will not apply any changes.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemablePictureBox", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemablePicturBox", "Theming is disabled.");
                return;
            }

            _themeColors = colors; //Store here to allow ThemColor to not be null
            BackColor = colors.BackColor;
            ForeColor = colors.ForeColor;
            _borderColor = colors.BorderColor;
            _accentColor = colors.AccentColor;

            Invalidate(); // Force a redraw to apply the new theme colors

        }
        #endregion IThemableControl Implementation
    }
}
