using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    public class ThemableCheckBox : CheckBox, IThemableControl
    {

        #region Fields and Properties
        private string _version = "V1.0";
        private Color _accentColor = SystemColors.Highlight;
        private Color _borderColor = SystemColors.ControlDark;
        private int _textPadding = 6;
        private ThemeColors _themeColors = new();
        private bool _useThemeColors = true;

        /// <summary>
        /// Gets the unique identifier for the control, including the version information.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableCheckBox {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether theme colors should be used for the control's appearance.
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
        /// Gets or sets a value inicating what <see cref="Color"/> to use for the <see cref="AccentColor"/>
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
        /// Gets or set a value indicating what <see cref="Color"> to use for the box portion of the checkbox
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
        /// Gets or set a value indicating the ammount of padding between where the check box ends and where the text starts
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Layout")]
        [Description("The padding between the checkbox and the text.")]
        [DefaultValue(6)]
        public int TextPadding
        {
            get => _textPadding;
            set { _textPadding = value; Invalidate(); }
        }
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableCheckBox"/> class with default styles.
        /// </summary>
        /// <remarks>This constructor sets various control styles to enhance rendering performance and
        /// appearance. It enables user painting, double buffering, and optimized painting to reduce flicker. The
        /// control is also set to automatically resize based on its content.</remarks>
        public ThemableCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.ResizeRedraw, true);
            AutoSize = true;
        }
        #endregion Constructor

        #region Methods and Events

        /// <summary>
        /// Handles the painting of the control, including background, text, and custom visual elements.
        /// </summary>
        /// <remarks>This method customizes the rendering of the control by drawing the background, text,
        /// images, and additional visual elements such as focus indicators.<para> Derived classes should call the base implementation to
        /// ensure proper rendering of the control's visual elements. </para></remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the data for the <see cref="OnPaint"/> event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            int boxSize = Font.Height - 2;
            var boxRect = new Rectangle(0, (Height - boxSize) / 2, boxSize, boxSize);

            // Draw box
            using var borderPen = new Pen(_borderColor, 1.6f);
            g.DrawRectangle(borderPen, boxRect);

            // Fill check if checked
            if (Checked)
            {
                using var brush = new SolidBrush(_accentColor);
                var inner = Rectangle.Inflate(boxRect, -4, -4);
                g.FillRectangle(brush, inner);
            }

            if (Focused)
            {
                ControlPaint.DrawFocusRectangle(g, new Rectangle(2, 2, Width - 4, Height - 4));
            }

            // Draw text
            var textSize = TextRenderer.MeasureText(Text, Font);
            int textY = boxRect.Top + (boxRect.Height - textSize.Height) / 2;
            var textPoint = new Point(boxRect.Right + _textPadding, textY);
            TextRenderer.DrawText(g, Text, Font, textPoint, ForeColor);
        }


        /// <summary>
        /// Raises the <see cref="Control.EnabledChanged"/> event.
        /// </summary>
        /// <remarks>This method is called when the <see cref="Control.Enabled"/> property value changes. 
        /// It applies the current theme colors to the control when the enabled state changes.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ApplyTheme(_themeColors);
        }

        #endregion Methods and Events

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control, updating its appearance based on the current state.
        /// </summary>
        /// <remarks>This method updates the control's foreground, background, border, and accent colors
        /// based on the provided theme colors. If theming is disabled or the <paramref name="colors"/> parameter is
        /// <see langword="null"/>, the method will log a message and no changes will be applied. The control is
        /// invalidated after applying the theme to ensure it is redrawn with the updated appearance.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be <see langword="null"/>.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableCheckBox", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableCheckBox", "Theming is disabled.");
                return;
            }
            _themeColors = colors;
            BackColor = colors.BackColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            AccentColor = Enabled ? colors.AccentColor : colors.DisabledColor;
            BorderColor = Enabled ? colors.BorderColor : colors.DisabledColor;
        }
        #endregion IThemableControl Implementation
    }
}
