using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    public class ThemableTextBox : TextBox, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private Color _borderColor = Color.Gray;
        private Color _accentColor = Color.DodgerBlue;
        private bool _useAccentBorder = false;
        private bool _focused = false;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();
        private bool _isPassword = false;

        /// <summary>
        /// Gets the unique identifier for the control, which includes the version information.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableTextBox {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether the text box is a password field.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether the text box is a password field.")]
        [DefaultValue(false)]
        public bool PasswordField
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                if (_isPassword)
                {
                    UseSystemPasswordChar = true;
                }
                else
                {
                    UseSystemPasswordChar = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the system password character should be used.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseSystemPasswordChar
        {
            get => base.UseSystemPasswordChar;
            set => base.UseSystemPasswordChar = value;
        }

        /// <summary>
        /// Gets or sets the border style of the control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get => base.BorderStyle;
            set => base.BorderStyle = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether theme colors are used.
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
        /// Gets or sets a value indicating whether the accent color is used for the border when the control is focused.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Whether to use the accent color when the control is focused.")]
        [DefaultValue(false)]
        public bool UseAccentBorder
        {
            get => _useAccentBorder;
            set { _useAccentBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled.
        /// </summary>
        /// <remarks>This property overrides the base <c>Enabled</c> property to maintain compatibility
        /// with custom colors. Use the <c>ReadOnly</c> property to simulate a disabled state.</remarks>
        [Browsable(false)]
        [Category("BazthalLib - Control")]
        [Description("Causes Incpatility with custom colors... Use ReadOnly instead")]
        [DefaultValue(true)]
        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = true; // Always keep base enabled
                ReadOnly = !value;   // Simulate disabled by setting ReadOnly
            }
        }
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableTextBox"/> class with optimized double buffering and a
        /// fixed single border style.
        /// </summary>
        /// <remarks>This constructor sets the control styles to optimize rendering performance and
        /// ensures that the text box redraws itself when resized. The border style is set to a fixed single line,
        /// matching the default system text box appearance.</remarks>
        public ThemableTextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.FixedSingle; // Use default system text box style
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the event when the control gains focus.
        /// </summary>
        /// <remarks>This method sets the control's focus state and triggers a repaint. It is called
        /// automatically when the control receives focus.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _focused = true;
            Invalidate();
        }

        /// <summary>
        /// Handles the event when the control loses focus.
        /// </summary>
        /// <remarks>This method sets the internal focus state to <see langword="false"/> and invalidates
        /// the control to trigger a repaint.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _focused = false;
            Invalidate();
        }

        /// <summary>
        /// Processes Windows messages and draws a custom border for specific messages.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="WndProc"/> method to handle custom drawing
        /// for specific Windows messages. It draws a custom border when the message is either WM_PAINT (0xF) or
        /// WM_NCPAINT (0x85).</remarks>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0xF || m.Msg == 0x85)
            {
                DrawCustomBorder();
            }
        }

        /// <summary>
        /// Draws a custom border around the control.
        /// </summary>
        /// <remarks>This method draws a border using the specified border color or accent color if the
        /// control is focused. It requires the control's handle to be created before execution.</remarks>
        private void DrawCustomBorder()
        {
            if (!IsHandleCreated) return;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using Pen pen = new Pen(_useAccentBorder && _focused ? _accentColor : _borderColor, 1);
                g.DrawRectangle(pen, rect);
            }
        }

        /// <summary>
        /// Handles the event when the enabled state of the control changes.
        /// </summary>
        /// <remarks>This method updates the read-only state of the control based on its enabled state.
        /// When the control is disabled, it becomes read-only.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            DebugUtils.Log("EnableChange", $"ThemableTextBox", "Enabled is set to {Enabled}.");

            if (!Enabled)
            {
                ReadOnly = true;
            }
            else
            {
                ReadOnly = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="ReadOnlyChanged"/> event.
        /// </summary>
        /// <remarks>This method logs the change in the read-only state and applies the current theme
        /// colors.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnReadOnlyChanged(EventArgs e)
        {
            base.OnReadOnlyChanged(e);
            DebugUtils.Log("ReadOnlyChange", $"ThemableTextBox", "ReadOnly is set to {ReadOnly}.");
            ApplyTheme(_themeColors);
        }
        #endregion Methods and Events

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's background, foreground, border, and accent colors
        /// based on the provided theme colors. If the control is read-only, the disabled color from the theme is used
        /// for these properties instead. The method does nothing if theming is disabled or if the <paramref
        /// name="colors"/> parameter is null.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableMaskedTextBox", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableMaskedTextBox", "Theming is disabled.");
                return;
            }
            _themeColors = colors;
            BackColor = colors.BackColor;
            ForeColor = ReadOnly ? colors.DisabledColor : colors.ForeColor;
            BorderColor = ReadOnly ? colors.DisabledColor : colors.BorderColor;
            AccentColor = ReadOnly ? colors.DisabledColor : colors.AccentColor;
            Invalidate();
        }
        #endregion IThemableControl Implementation
    }
}
