using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    public class ThemableRadioButton : RadioButton, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private ThemeColors _themeColors = new();
        private Color _accentColor = SystemColors.Highlight;
        private Color _borderColor = SystemColors.ControlDark;
        private int _textPadding = 6;
        private bool _useThemeColors = true;

        /// <summary>
        /// Gets the unique identifier for the ThemableRadioButton control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableRadioButton {_version}";

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
        /// Gets or sets the color used for the accent when the control is checked.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the accent when checked.")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
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
        /// Gets or sets the padding between the radio button border and the text.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Layout")]
        [Description("The padding between the radio button border and the text.")]
        [DefaultValue(6)]
        public int TextPadding
        {
            get => _textPadding;
            set { _textPadding = value; Invalidate(); }
        }
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableRadioButton"/> class with custom painting styles.
        /// </summary>
        /// <remarks>This constructor sets the control to use user-defined painting, optimizes double
        /// buffering, and enables automatic resizing.</remarks>
        public ThemableRadioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = true;
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        ///  Handles the event when the enabled state of the control changes.
        /// </summary>
        /// <remarks>This method applies the current theme colors to the control whenever its enabled
        /// state changes.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ApplyTheme(_themeColors);
        }
        /// <summary>
        /// Renders the control's visual appearance, including its border, fill, and text.
        /// </summary>
        /// <remarks>This method is responsible for drawing the control's border and fill when it is
        /// checked, as well as rendering the associated text. It uses anti-aliasing for smoother graphics.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int radius = 16;
            Rectangle outer = new Rectangle(0, (Height - radius) / 2, radius, radius);

            using var borderPen = new Pen(_borderColor);
            using var fillBrush = new SolidBrush(_accentColor);

            g.DrawEllipse(borderPen, outer);

            if (Checked)
            {
                Rectangle inner = new Rectangle(
                    outer.X + 4, outer.Y + 4, radius - 8, radius - 8);
                g.FillEllipse(fillBrush, inner);
            }


            // Draw text
            int textY = outer.Top + (outer.Height - TextRenderer.MeasureText(Text, Font).Height) / 2;
            TextRenderer.DrawText(g, Text, Font,
                new Point(outer.Right + _textPadding, textY),
                ForeColor, TextFormatFlags.Left);
        }
        #endregion Methods and Events

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's background, foreground, accent, and border colors
        /// based on the provided theme colors. If the control is disabled, the disabled color from the theme is used
        /// for the foreground, accent, and border colors. The method does nothing if <paramref name="colors"/> is null
        /// or if theme colors are not in use.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!UseThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableRadioButton", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableRadioButton", "Skipping theme assignment");
                return;
            }
            _themeColors = colors;

            BackColor = colors.BackColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            _accentColor = Enabled ? colors.AccentColor : colors.DisabledColor;
            _borderColor = Enabled ? colors.BorderColor : colors.DisabledColor;

            Invalidate(); // Force redraw
        }
        #endregion IThemableControl Implementation
    }
}
