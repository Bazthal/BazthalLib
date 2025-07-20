using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{

    [ToolboxItem(false)] //this is a base class and should not be added to the toolbox
    [DesignerCategory("code")]
    [DefaultEvent("Click")]
    public class ThemableControlBase : Control, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private Color _borderColor = SystemColors.ActiveBorder;
        private Color _accentColor = Color.DodgerBlue;
        private Color _selectedItemBackColor = SystemColors.Highlight;
        private Color _selectedItemForeColor = SystemColors.HighlightText;

        private bool _enableBorder = true;
        private bool _roundedCorners = false;
        private int _cornerRadius = 5;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();

        private TextRenderMode _textRenderMode = TextRenderMode.None;

        /// <summary>
        /// Gets the unique identifier for the control, which includes the version information.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableControlBase {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether global theme colors override the styling.
        /// </summary>
        /// <remarks>Set this property to <see langword="false"/> if you are using the designer to set the
        /// colors manually.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to allow the global theme colors to override styleing - Disable if using designer to set the colors")]
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
        /// Gets or sets the color used for the accent in the appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the accent")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the background color of the selected item.
        /// </summary>
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the selected item background.")]
        [DefaultValue(typeof(Color), "LightGray")]
        public Color SelectedItemBackColor
        {
            get => _selectedItemBackColor;
            set { _selectedItemBackColor = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets the color of the text for the selected item.
        /// </summary>
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the selected item text.")]
        [DefaultValue(typeof(Color), "Black")]
        public Color SelectedItemForeColor
        {
            get => _selectedItemForeColor;
            set { _selectedItemForeColor = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled.
        /// </summary>
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("Enable Border or not")]
        [DefaultValue(true)]
        public bool EnableBorder { get => _enableBorder; set => _enableBorder = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the corners of the control should be rounded.
        /// </summary>
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("Allow for round corners or not")]
        [DefaultValue(false)]
        public bool RoundCorners { get => _roundedCorners; set => _roundedCorners = value; }

        /// <summary>
        /// Gets or sets the radius for rounding the corners of the element.
        /// </summary>
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("Radius for the corner rounding")]
        [DefaultValue(5)]
        public int CornerRadius { get => _cornerRadius; set => _cornerRadius = value; }

        /// <summary>
        /// Gets or sets the mode used to render text within the control.
        /// </summary>
        /// <remarks>Changing this property will cause the control to be redrawn to reflect the new text
        /// rendering mode.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Category("BazthalLib - Appearance")]
        [Description("Specifies how text should be rendered within the control.")]
        public virtual TextRenderMode TextRenderMode
        {
            get => _textRenderMode;
            set { _textRenderMode = value; Invalidate(); }
        }


        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableControlBase"/> class with specific control styles.
        /// </summary>
        /// <remarks>This constructor sets various control styles to enhance rendering performance and
        /// visual appearance. It enables double buffering, custom painting, and support for transparent backgrounds.
        /// Additionally, it attaches event handlers to invalidate the control when it gains or loses focus, or when the
        /// mouse button is pressed, ensuring the control is redrawn appropriately.</remarks>
        public ThemableControlBase()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            this.GotFocus += (s, e) => Invalidate();
            this.LostFocus += (s, e) => Invalidate();
            this.MouseDown += (s, e) => { Focus(); Invalidate(); };

        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the painting of the control, including optional border and text rendering.
        /// </summary>
        /// <remarks>This method is overridden to provide custom painting logic. If <see
        /// cref="EnableBorder"/> is <see langword="true"/>, a border is drawn around the control. If <see
        /// cref="TextRenderMode"/> is not <see cref="TextRenderMode.None"/> and the control's text is not empty, the
        /// text is rendered.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data for the paint operation.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (EnableBorder)
            {
                DrawBorder(e.Graphics);
            }

            if (TextRenderMode != TextRenderMode.None && !string.IsNullOrEmpty(base.Text))
            {
                DrawTextContent(e.Graphics);
            }
        }

        /// <summary>
        /// Draws a border around the control using the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <remarks>If <see cref="RoundCorners"/> is <see langword="true"/>, the border will be drawn
        /// with rounded corners using the specified <see cref="CornerRadius"/>. In design mode, a dashed border is
        /// drawn for preview purposes. Otherwise, a solid border is drawn using the specified <see
        /// cref="BorderColor"/>.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the border.</param>
        protected virtual void DrawBorder(Graphics g)
        {

            if (RoundCorners)
            {
                // Define the rectangle and corner radius
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                // Create a rounded rectangle path
                using GraphicsPath path = new GraphicsPath();
                path.AddArc(rect.X, rect.Y, CornerRadius, CornerRadius, 180, 90);
                path.AddArc(rect.Right - CornerRadius, rect.Y, CornerRadius, CornerRadius, 270, 90);
                path.AddArc(rect.Right - CornerRadius, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                path.CloseFigure();

                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Optional: Designer preview of rounded border
                if (DesignMode)
                {

                    using Pen previewPen = new Pen(Color.DarkGray, 1)
                    {
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                    };
                    g.DrawPath(previewPen, path);
                }
                else
                {
                    using Pen pen = new Pen(BorderColor, 4);
                    g.DrawPath(pen, path);
                    Region = new Region(path);
                }
            }
            else
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                if (DesignMode)
                {
                    using Pen previewPen = new Pen(Color.DarkGray, 1)
                    {
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                    };
                    g.DrawRectangle(previewPen, rect);
                }
                else
                {
                    using Pen pen = new Pen(BorderColor, 4);
                    g.DrawRectangle(pen, rect);
                    Region = new Region(rect);
                }
            }

        }

        /// <summary>
        /// Renders the text content on the specified graphics surface using the current text rendering mode.
        /// </summary>
        /// <remarks>The text is drawn within a rectangle that is slightly inset from the control's edges.
        /// The rendering mode determines the alignment of the text. If <see cref="TextRenderMode"/> is set to <see
        /// cref="TextRenderMode.None"/>, the method returns without drawing any text.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object on which the text will be drawn.</param>
        protected virtual void DrawTextContent(Graphics g)
        {
            StringFormat sf = new()
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            switch (TextRenderMode)
            {
                case TextRenderMode.Centered:
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    break;

                case TextRenderMode.TopLeft:
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Near;
                    break;


                //Removed Custom from options as it's not going to be used going forward for now it's just commented out for development and
                //Could be implemented if i change my mind in the future
                /*
                case TextRenderMode.Custom:
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Near;
                break; */

                case TextRenderMode.None:
                default:
                    return;
            }

            Rectangle textRect = new Rectangle(4, 4, Width - 8, Height - 8);
            using Brush brush = new SolidBrush(ForeColor);

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.DrawString(Text, Font, brush, textRect, sf);
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
        /// <remarks>This method updates the control's foreground, background, border, and accent colors
        /// based on the provided theme colors. If the control is disabled, it uses the disabled color variants from the
        /// theme.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. If <paramref name="colors"/> is <see
        /// langword="null"/>, the method returns without making changes.</param>
        public virtual void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
            {
                return;
            }

            _themeColors = colors;

            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            BackColor = colors.BackColor;
            BorderColor = Enabled ? colors.BorderColor : colors.DisabledColor;
            AccentColor = Enabled ? colors.AccentColor : colors.DisabledColor;
            SelectedItemBackColor = colors.SelectedItemBackColor;
            SelectedItemForeColor = colors.SelectedItemForeColor;
            Invalidate(); // Force redraw
        }
        #endregion IThemableControl Implementation
    }

}