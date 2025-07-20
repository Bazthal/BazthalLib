using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    public class ThemableGroupBox : GroupBox, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private ThemeColors _themeColors = new();
        private bool _useThemeColors = true;

        /// <summary>
        /// Gets the unique identifier for the ThemableGroupBox control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableGroupBox {_version}";

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
        [DefaultValue(typeof(Color), "ActiveBorder")]
        public Color BorderColor { get; set; } = SystemColors.ActiveBorder;

        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableGroupBox"/> class with optimized rendering settings.
        /// </summary>
        /// <remarks>This constructor sets the control styles to enable user painting, optimized double
        /// buffering, and redraw on resize. These settings improve rendering performance and visual appearance when the
        /// control is resized or updated.</remarks>
        public ThemableGroupBox()
        {
            SetStyle( ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw , true);
            DoubleBuffered = true;
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the painting of the control, including drawing the border and text.
        /// </summary>
        /// <remarks>This method clears the background, draws a border around the control, and renders the
        /// text. The border is drawn with the specified <see cref="BorderColor"/>, and the text is drawn using the
        /// control's <see cref="Font"/> and <see cref="ForeColor"/>. The background behind the text is cleared to
        /// prevent overlap with the border.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            Size textSize = TextRenderer.MeasureText(Text, Font);

            Rectangle borderRect = new Rectangle(0, textSize.Height / 2, Width - 1, Height - textSize.Height / 2 - 1);

            using (Pen borderPen = new Pen(BorderColor))
            {
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }

            // Clear background behind the text so it doesn't get overlapped by the border
            using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
            {
                Rectangle textBgRect = new Rectangle(6, 0, textSize.Width, textSize.Height);
                e.Graphics.FillRectangle(backgroundBrush, textBgRect);
            }

            // Draw the text
            using (SolidBrush textBrush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(Text, Font, textBrush, 6, 0);
            }
        }
        /// <summary>
        /// Handles the event when the enabled state of the control changes.
        /// </summary>
        /// <remarks>This method applies the current theme colors to the control when its enabled state
        /// changes.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ApplyTheme(_themeColors);
        }
        #endregion Methods and Events

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's border, foreground, and background colors based on
        /// the provided theme colors. If theming is disabled or the <paramref name="colors"/> parameter is null, the
        /// method logs a message and does not apply the theme.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemeableGroupBox", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableGroupBox", "Theming is disabled.");
            }

            _themeColors = colors;

            BorderColor = Enabled ? colors.BorderColor : colors.DisabledColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            BackColor = colors.BackColor;
            Invalidate(); // Force redraw
        }

        #endregion IThemableControl Implementation
    }
}


