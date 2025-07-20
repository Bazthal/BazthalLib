using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    public class ThemableRichTextBox : RichTextBox, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private Color _borderColor = Color.Gray;
        private Color _accentColor = Color.DodgerBlue;
        private bool _useAccentBorder = false;
        private bool _focused = false;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableRichTextBox {_version}";

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
        /// Gets or sets a value indicating whether the control is enabled.
        /// </summary>
        /// <remarks>Setting this property to <see langword="false"/> will make the control read-only, 
        /// simulating a disabled state while keeping the base control enabled.</remarks>
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
        /// Handles the event when the control gains focus.
        /// </summary>
        /// <remarks>This method sets the control's internal focus state and triggers a redraw of the
        /// control.</remarks>
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
        #endregion Fields and Properties


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableRichTextBox"/> class with optimized double buffering
        /// and resize redraw capabilities.
        /// </summary>
        /// <remarks>This constructor sets the control styles to enhance rendering performance and visual
        /// updates. The border style is set to none, using the default system text box style.</remarks>
        public ThemableRichTextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.None; // Use default system text box style
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Processes Windows messages and draws a custom border for specific messages.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="WndProc"/> method to handle specific
        /// Windows messages. If the message is a paint message (0xF) or a non-client area paint message (0x85), it
        /// triggers the drawing of a custom border.</remarks>
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
        /// <remarks>This method logs the change in the enabled state and adjusts the read-only status of
        /// the control accordingly. When the control is disabled, it becomes read-only; when enabled, it is no longer
        /// read-only.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            DebugUtils.Log("EnableChange", $"ThemableRichTextBox", "Enabled is set to {Enabled}.");
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
            DebugUtils.Log("ReadonlyChange", $"ThemableRichTextBox", "ReadOnly is set to {ReadOnly}.");
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
                DebugUtils.LogIf(colors == null, "Theming", "ThemableRichTextBox", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableRichTextBox", "Theming is disabled.");
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
