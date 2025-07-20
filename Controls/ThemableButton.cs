using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{


    public class ThemableButton : Button, IThemableControl
    {
        #region Fields
        private readonly string _version = "V1.4";
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();
        private bool _roundedCorners = true;
        private Color _accentColor = Color.DodgerBlue;
        private Color _borderColor = Color.Gray;
        private bool _enableBorder = true; // Whether to show the border or not
        private int _cornerRadius = 5; // Default corner radius, can be adjusted
        private bool _isMouseDown = false; // Track mouse down state
        private bool _useAccentColor = true; // Whether to use accent color or not
        private bool _useAccentForUnderline = false; // Whether to use accent color for underline or not
        private bool _useTintedImage = false; // Whether to use tinted image or not
        private Image _tintedImage = null; // The tinted image to be used if UseTintedImage is true
        private bool _useAccentForTintedImage = false; // Whether to use accent color for tinted image or not
        private bool _drawUnderline = false; // Whether to draw underline on text or not
        private bool _allowFocus = true; // Whether to allow focus on the button or not
        private bool _allowClickHighlight = true; // Whether to allow click highlight or not
        private bool _useTightFocusBorder = true; // Whether to have padding or not
        private bool _focusWrapAroundImage = false; // Whether to wrap around the image or not
        private bool _matchImageSize = true; // Whether to match the image size with the button size or not
        private ContentAlignment _imageAlign = ContentAlignment.MiddleCenter;

        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets the unique identifier for the control, including the version information.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableButton {_version}";

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
        /// Gets or sets a value indicating whether the control allows focus.
        /// </summary>
        /// <remarks>When set to <see langword="false"/>, the control will not be selectable and will be
        /// excluded from the tab order.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to allow Focus effect or not.")]
        [DefaultValue(true)]
        public bool AllowFocus
        {
            get => _allowFocus;
            set
            {
                _allowFocus = value;
                if (!value)
                {
                    this.SetStyle(ControlStyles.Selectable, false);
                    this.TabStop = false; // Disable tab stop if focus is not allowed
                }
                else
                {
                    this.SetStyle(ControlStyles.Selectable, true);
                    this.TabStop = true; // Enable tab stop if focus is allowed
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control should use a tight focus border with no padding.
        /// </summary>
        /// <remarks>Changing this property will cause the control to redraw to reflect the updated
        /// appearance.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to allow no padding or not.")]
        [DefaultValue(true)]
        public bool UseTightFocusBorder
        {
            get => _useTightFocusBorder;
            set
            {
                _useTightFocusBorder = value;
                Invalidate(); // Force redraw to apply changes
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the focus box wraps around the image.
        /// </summary>
        /// <remarks>Changing this property will trigger a redraw of the control to reflect the updated
        /// focus behavior.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to wrap around the image with the focus box or not.")]
        [DefaultValue(false)]
        public bool FocusWrapAroundImage
        {
            get => _focusWrapAroundImage;
            set
            {
                _focusWrapAroundImage = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button size should match the size of the tinted image.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, the button will automatically resize to match the
        /// dimensions of the tinted image. Changing this property triggers a button resize and a redraw to reflect the
        /// updated size.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to match the button size with the tinted image size or not.")]
        [DefaultValue(true)]
        public bool MatchImageSize
        {
            get => _matchImageSize;
            set
            {
                _matchImageSize = value;
                ResizeButtonToImage();
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow for the button to highligt on click
        /// </summary>
        /// <remarks> when set to <see langword="true"/> the button would show a highlight when clicked</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to allow click highlight effect or not.")]
        [DefaultValue(true)]
        public bool AllowClickHighlight
        {
            get => _allowClickHighlight;
            set
            {
                _allowClickHighlight = value;
            }
        }

        /// <summary>
        /// Gets or sets the tinted image to be used.
        /// </summary>
        /// <remarks>Setting this property automatically adjusts the button size to match the image
        /// dimensions and triggers a redraw to apply the changes.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The tinted image to be used.")]
        [DefaultValue(null)]
        public Image TintedImage
        {
            get => _tintedImage;
            set
            {
                _tintedImage = value;
                _useTintedImage = value != null; // Automatically set UseTintedImage based on whether TintedImage is set
                ResizeButtonToImage();
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether to draw the underline at the bottom of the button
        /// </summary>
        //Underline is used to draw a line at the bottom of the button to indicate an active state or for visual emphasis
        [Browsable(false)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to draw the underline or not.")]
        [DefaultValue(false)]
        public bool DrawUnderline
        {
            get => _drawUnderline;
            set
            {
                _drawUnderline = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        ///  Gets or sets a value indicating whether to use the <see cref="BorderColor"/> or <see cref="AccentColor"/> for the underline
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Wether to Use Accent for underline color or border")]
        [DefaultValue(false)]
        public bool UseAccentForUnderline
        {
            get => _useAccentForUnderline;
            set
            {
                _useAccentForUnderline = value;
                Invalidate(); // Force redraw to apply changes
            }
        }
        /// <summary>
        /// Gets or set a value indicating what <see cref="Color"> to use for the <see cref="EnableBorder">
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Color of the border")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether to use the <see cref="AccentColor"/> or <see cref="ForeColor"/> for the color of the <see cref="TintedImage"/>
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use accent color for tinted image or not.")]
        [DefaultValue(false)]
        public bool UseAccentForTintedImage
        {
            get => _useAccentForTintedImage;
            set
            {
                _useAccentForTintedImage = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets a value inicating what <see cref="Color"/> to use for the <see cref="AccentColor"/>
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The accent color for the button.")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or set a value indication whether to use <see cref="AccentColor"/> or <see cref="Control.BackColor"/> for <see cref="AllowClickHighlight"/>
        /// </summary>

        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use the accent color or not.")]
        [DefaultValue(true)]
        public bool UseAccentColor
        {
            get => _useAccentColor;
            set
            {
                _useAccentColor = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether or not to <see cref="DrawCustomBorder(PaintEventArgs)"
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to draw the border")]
        [DefaultValue(true)]
        public bool EnableBorder
        {
            get => _enableBorder;
            set
            {
                _enableBorder = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to draw rounded corders
        /// </summary>

        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use round corners or not.")]
        [DefaultValue(true)]
        public bool RoundCorners
        {
            get => _roundedCorners;
            set
            {
                _roundedCorners = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets a value indcating the raduis of the <see cref="RoundCorners"/>
        /// </summary>

        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The corner radius for rounded corners.")]
        [DefaultValue(5)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                Invalidate(); // Force redraw to apply changes
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)] //Hide from Intellisense and property grid
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set => base.FlatStyle = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)] //Hide from Intellisense and property grid
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new RightToLeft RightToLeft // Hide RightToLeft property since it's half implemented in Button by default 
        {
            get => base.RightToLeft;
            set => base.RightToLeft = value;
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableButton"/> class with default styling settings.
        /// </summary>
        /// <remarks>The constructor sets the button to use a flat style and removes the border size to
        /// allow for custom theming. It also enables optimized double buffering and redraw on resize for smoother
        /// rendering. Mouse down and up events are handled to trigger a redraw, allowing for visual feedback when the
        /// button is pressed.</remarks>
        public ThemableButton()
        {
            FlatStyle = FlatStyle.Flat; //Default to flat style to block system from overriding colors
            FlatAppearance.BorderSize = 0; // Remove border size to allow custom theming
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.UserPaint, true);

            this.MouseDown += (s, e) =>
            {
                _isMouseDown = true;
                this.Invalidate(); // Redraw
            };

            this.MouseUp += (s, e) =>
            {
                _isMouseDown = false;
                this.Invalidate(); // Redraw
            };
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Determines a highlight color based on the brightness of the specified base color.
        /// </summary>
        /// <param name="baseColor">The base <see cref="Color"/> used to calculate the highlight color.</param>
        /// <returns>A lighter or darker variation of the <paramref name="baseColor"/> depending on its brightness. If the
        /// brightness of <paramref name="baseColor"/> is less than 0.25, a lighter color is returned; otherwise, a
        /// darker color is returned.</returns>
        private Color GetClickHighlightColor(Color baseColor)
        {
            return baseColor.GetBrightness() < 0.25f
                ? ControlPaint.Light(baseColor, 0.3f)
                : ControlPaint.Dark(baseColor, 0.2f);
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Forms.Control.HandleCreated"/> event.
        /// </summary>
        /// <remarks>This method ensures that the button is resized to match the dimensions of its
        /// associated image if an image is set, after the handle is created. Derived classes should call the base implementation to ensure
        /// proper event handling.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ResizeButtonToImage();
        }

        /// <summary>
        /// Handles the resize event of the control and adjusts the button size to match the image dimensions.
        /// </summary>
        /// <remarks>This method ensures that the button is resized appropriately when the control's size
        /// changes. It calls the base implementation of <see cref="OnResize"/> and then adjusts the button size to
        /// align with the associated image.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeButtonToImage();
        }

        /// <summary>
        /// Handles the painting of the control, including background, text, images, and custom visual elements.
        /// </summary>
        /// <remarks>This method customizes the rendering of the control by drawing the background, text,
        /// images, and additional visual elements such as underlines and focus indicators. It also applies visual
        /// effects based on the control's state, such as mouse interaction or focus. <para> If a tinted image is used,
        /// it is rendered with a tint color determined by the control's properties. Otherwise, the control's background
        /// and text are drawn. Additional elements, such as an underline or focus rectangle, are drawn based on the
        /// control's configuration and state. </para> <para> Derived classes should call the base implementation to
        /// ensure proper rendering of the control's visual elements. </para></remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the data for the <see cref="OnPaint"/> event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            if (_useTintedImage && _tintedImage != null)
            {
                Color baseTintColor = _useAccentForTintedImage ? _accentColor : ForeColor;

                if (DesignMode && baseTintColor.ToArgb() == Color.Black.ToArgb())
                {
                    baseTintColor = Color.White;
                }

                if (_isMouseDown && _allowClickHighlight)
                {
                    baseTintColor = GetClickHighlightColor(baseTintColor);
                }

                TintedImageRenderer.Draw(e.Graphics, _tintedImage, baseTintColor, ClientRectangle, _imageAlign);
            }
            else
            {
                Color fillColor = BackColor;

                if (_isMouseDown && _allowClickHighlight)
                {
                    fillColor = _useAccentColor
                        ? _accentColor
                        : GetClickHighlightColor(BackColor);
                }

                e.Graphics.FillRectangle(new SolidBrush(fillColor), ClientRectangle);
                DrawText(e);
            }

            // Underline
            if (_drawUnderline)
            {
                int thickness = 3;
                int y = Height - thickness;
                using var pen = new Pen(_useAccentForUnderline ? _accentColor : _borderColor, thickness);
                e.Graphics.DrawLine(pen, 0, y, Width, y);
            }



            if (Focused && _useTintedImage && _tintedImage != null && _focusWrapAroundImage)
            {
                using GraphicsPath path = TintedImageRenderer.GetOpaqueOutlinePath(
                    _tintedImage, ClientRectangle, _imageAlign, 10, _useTightFocusBorder ? 1.02f : 1.0f);

                if (path != null)
                {
                    Color focusColor = GetClickHighlightColor(ForeColor);
                    using Pen pen = new Pen(focusColor, 1) { DashStyle = DashStyle.Dot };
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                if (Focused && _allowFocus)
                    ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(2, 2, Width - 4, Height - 4));
            }

            DrawCustomBorder(e);
        }

        /// <summary>
        /// Draws the control's text within its client area using the specified alignment and formatting options.
        /// </summary>
        /// <remarks>The text is rendered based on the control's <see cref="Text"/>, <see cref="Font"/>,
        /// <see cref="ForeColor"/>,  and <see cref="TextAlign"/> properties. The alignment of the text is determined by
        /// the <see cref="TextAlign"/>  property, which maps to corresponding <see cref="TextFormatFlags"/>
        /// values.</remarks>
        /// <param name="e">The <see cref="PaintEventArgs"/> containing the graphics context used for rendering.</param>
        private void DrawText(PaintEventArgs e)
        {

            TextFormatFlags flags = TextFormatFlags.Default;

            switch (this.TextAlign)
            {
                case ContentAlignment.TopLeft:
                    flags = TextFormatFlags.Left | TextFormatFlags.Top;
                    break;
                case ContentAlignment.TopCenter:
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
                    break;
                case ContentAlignment.TopRight:
                    flags = TextFormatFlags.Right | TextFormatFlags.Top;
                    break;
                case ContentAlignment.MiddleLeft:
                    flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.MiddleCenter:
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.MiddleRight:
                    flags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.BottomLeft:
                    flags = TextFormatFlags.Left | TextFormatFlags.Bottom;
                    break;
                case ContentAlignment.BottomCenter:
                    flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
                    break;
                case ContentAlignment.BottomRight:
                    flags = TextFormatFlags.Right | TextFormatFlags.Bottom;
                    break;
            }

            TextRenderer.DrawText(
                e.Graphics,
                this.Text,
                this.Font,
                this.ClientRectangle,
                this.ForeColor,
                flags
            );

        }
        
        /// <summary>
        /// /// Adjusts the size of the button to match the dimensions of the tinted image, if applicable.
        /// /// </summary>
        /// /// <remarks>This method resizes the button to match the width and height of the tinted image when both <see
        /// /// cref="_useTintedImage"/> is <see langword="true"/> and <see cref="_tintedImage"/> is not <see langword="null"/>. The
        /// /// resizing only occurs if <see cref="_matchImageSize"/> is also <see langword="true"/>.</remarks>
        private void ResizeButtonToImage()
        {
            if (_useTintedImage && _tintedImage != null && _matchImageSize)
            {
                this.Width = _tintedImage.Width;
                this.Height = _tintedImage.Height;
            }
        }

        /// <summary>
        /// Draws a custom border around the control, optionally with rounded corners, based on the current settings.
        /// </summary>
        /// <remarks>The border appearance is determined by the control's internal settings, such as
        /// whether rounded corners are enabled, the corner radius, the border color, and whether the border is enabled.
        /// In design mode, a dashed preview border is drawn instead of the final border. The method also adjusts the
        /// control's clipping region to match the border shape.</remarks>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data for the paint operation.</param>
        private void DrawCustomBorder(PaintEventArgs e)
        {
            if (_roundedCorners)
            {
                // Define the rectangle and corner radius
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                // Create a rounded rectangle path
                using GraphicsPath path = new GraphicsPath();
                path.AddArc(rect.X, rect.Y, _cornerRadius, _cornerRadius, 180, 90);
                path.AddArc(rect.Right - _cornerRadius, rect.Y, _cornerRadius, _cornerRadius, 270, 90);
                path.AddArc(rect.Right - _cornerRadius, rect.Bottom - _cornerRadius, _cornerRadius, _cornerRadius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - _cornerRadius, _cornerRadius, _cornerRadius, 90, 90);
                path.CloseFigure();

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Optional: Designer preview of rounded border
                if (DesignMode)
                {

                    using Pen previewPen = new Pen(Color.DarkGray, 1)
                    {
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                    };
                    e.Graphics.DrawPath(previewPen, path);
                }
                else
                {

                    using Pen pen = new Pen(_borderColor, 4);
                    if (_enableBorder)
                        e.Graphics.DrawPath(pen, path); //Draw border with rounded corners if enabled
                    Region = new Region(path); // Set the region to the rounded rectangle path to clip the button with / without border

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
                    e.Graphics.DrawRectangle(previewPen, rect);
                }
                else
                {
                    using Pen pen = new Pen(_borderColor, 4);
                    if (_enableBorder)
                        e.Graphics.DrawRectangle(pen, rect); // Draw border with sharp corners if enabled

                    Region = new Region(rect); // Set the region to the rectangle to clip the button with / without border
                }

            }

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
        #endregion

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
                DebugUtils.LogIf(colors == null, "Theming", "ThemableButton", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableButton", " Theming is disabled.");
                return;
            }
            _themeColors = colors;
            _borderColor = Enabled ? colors.BorderColor : colors.DisabledColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            BackColor = colors.BackColor;
            _accentColor = colors.AccentColor; // Update accent color from theme
            Invalidate(); // Force redraw
        }

        #endregion IThemableControl Implementation


    }
}
