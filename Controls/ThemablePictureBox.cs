using BazthalLib.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BazthalLib.DebugUtils;

namespace BazthalLib.Controls
{

    public class ThemablePictureBox : PictureBox, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.1";

        private Color _borderColor = Color.Gray;
        private Color _accentColor = Color.DodgerBlue;
        private bool _useThemeColors = true;
        private bool _useAccentColor = false;
        private bool _enableBorder = true;
        private ThemeColors _themeColors = new();

        private bool _useTintedImage = false;
        private bool _autoScaleTintedImage = true;
        private Color _tintColor = Color.White;

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
        /// Gets or sets a value indicating whether the tinted image renderer is used for images.
        /// </summary>
        /// <remarks>When enabled, images will be rendered using a tinted effect. This property is part of
        /// the  "BazthalLib - Appearance" category and can be configured at design time.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to use the tinted image renderer for images.")]
        [DefaultValue(false)]
        public bool UseTintedImage
        {
            get => _useTintedImage;
            set { _useTintedImage = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the image is automatically scaled  when using tinted image
        /// rendering.
        /// </summary>
        /// <remarks>When enabled, the image will be resized to fit the control's dimensions while
        /// maintaining  the tinted rendering effect. Disabling this property preserves the original image
        /// size.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to auto scale the image when using tinted image rendering.")]
        [DefaultValue(true)]
        public bool AutoScaleTintedImage
        {
            get => _autoScaleTintedImage;
            set { _autoScaleTintedImage = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the tint color used for rendering tinted images.
        /// </summary>
        /// <remarks>Changing this property will automatically refresh the control to reflect the updated
        /// tint color.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The tint color to use for tinted image rendering.")]
        [DefaultValue(typeof(Color), "White")]
        public Color TintColor
        {
            get => _tintColor;
            set { _tintColor = value; Invalidate(); }
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
        /// Handles the painting of the control, including rendering the image with optional tinting and scaling, and
        /// drawing a border if enabled.
        /// </summary>
        /// <remarks>This method customizes the painting behavior of the control. If a tinted image is
        /// enabled and an image is set, the image is rendered with the specified tint color and optional scaling. The
        /// tint color is determined by the accent color if enabled, or the default tint color otherwise. If the border
        /// is enabled, a border is drawn around the control using the specified border color or accent color. <para> If
        /// no custom image rendering is required, the base class's <see cref="Control.OnPaint(PaintEventArgs)"/> method
        /// is called to handle default painting behavior. </para></remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data for the paint operation.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_useTintedImage && Image != null)
            {
                Color tint = _useAccentColor ? _accentColor : _tintColor;

                Rectangle sizeModeRect = CalculateSizeModeRectangle(Image);

                float scale = 1.0f;
                if (_autoScaleTintedImage)
                {
                    scale = AutoScaleTintedImage ? CalculateDownscaleFactor(Image, sizeModeRect) : 1.0f;
                }

                TintedImageRenderer.Draw(e.Graphics, Image, tint, sizeModeRect, ContentAlignment.MiddleCenter, scale, ImageQuality.Smooth);
            }

            else {
                base.OnPaint(e);
            }


            if (_enableBorder)
            {
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using Pen pen = new Pen(_useAccentColor ? _accentColor : _borderColor, 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        /// <summary>
        /// Calculates the downscale factor required to fit an image within the specified bounds.
        /// </summary>
        /// <param name="img">The image to be scaled. Cannot be <see langword="null"/>.</param>
        /// <param name="bounds">The rectangular area within which the image should fit. The width and height must be greater than zero.</param>
        /// <returns>The scaling factor as a <see cref="float"/> that, when applied, ensures the image fits within the bounds.
        /// Returns <c>1.0f</c> if the image is <see langword="null"/> or the bounds are invalid.</returns>
        private float CalculateDownscaleFactor(Image img, Rectangle bounds)
        {
            if (img == null || bounds.Width <= 0 || bounds.Height <= 0)
                return 1.0f;

            float scaleX = (float)bounds.Width / img.Width;
            float scaleY = (float)bounds.Height / img.Height;

            return Math.Min(scaleX, scaleY);
        }

        /// <summary>
        /// Calculates the rectangle in which an image should be displayed based on the current <see
        /// cref="PictureBoxSizeMode"/>.
        /// </summary>
        /// <param name="img">The image to be displayed. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="Rectangle"/> representing the position and size of the image within the control. The rectangle
        /// is calculated according to the <see cref="PictureBoxSizeMode"/>: <list type="bullet">
        /// <item><description><see cref="PictureBoxSizeMode.StretchImage"/>: The image is stretched to fill the
        /// control's dimensions.</description></item> <item><description><see cref="PictureBoxSizeMode.CenterImage"/>:
        /// The image is centered within the control.</description></item> <item><description><see
        /// cref="PictureBoxSizeMode.Zoom"/>: The image is scaled proportionally to fit within the control while
        /// maintaining its aspect ratio.</description></item> <item><description><see
        /// cref="PictureBoxSizeMode.AutoSize"/>: The control is resized to match the image
        /// dimensions.</description></item> <item><description><see cref="PictureBoxSizeMode.Normal"/>: The image is
        /// displayed at its original size in the top-left corner of the control.</description></item> </list></returns>
        private Rectangle CalculateSizeModeRectangle(Image img)
        { 
            switch (SizeMode)
            {
                case PictureBoxSizeMode.StretchImage:
                    {
                        return new Rectangle(0, 0, Width, Height); 
                    }
                    case PictureBoxSizeMode.CenterImage:
                    {
                        return new Rectangle((Width - img.Width) / 2, (Height - img.Height) / 2, img.Width, img.Height); 
                    }
                    case PictureBoxSizeMode.Zoom:
                    {
                        float ratioX = (float)Width / img.Width;
                        float ratioY = (float)Height / img.Height;
                        float ratio = Math.Min(ratioX, ratioY);
                        int newWidth = (int)(img.Width * ratio);
                        int newHeight = (int)(img.Height * ratio);
                        int posX = (Width - newWidth) / 2;
                        int posY = (Height - newHeight) / 2;
                        return new Rectangle(posX, posY, newWidth, newHeight);
                    }
                    case PictureBoxSizeMode.AutoSize:
                    {
                        this.Size = img.Size;
                        return new Rectangle(0, 0, img.Width, img.Height); 
                    }
                    case PictureBoxSizeMode.Normal:
                    default:
                    {
                        return new Rectangle(0, 0, img.Width, img.Height);
                    }
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
                DebugUtils.LogIf(colors == null, "Theming", "ThemablePictureBox", "ThemeColors is null.", logLevel: LogLevel.Error);
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemablePicturBox", "Theming is disabled.", logLevel: LogLevel.Info);
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
