using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BazthalLib.UI;

namespace BazthalLib.Controls
{

    [DesignerCategory("Code")]
    [DefaultEvent("Click")]
    [ToolboxItem(false)]
    [DesignTimeVisible(true)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public class ThemableToolStripButton : ToolStripControlHost, IThemableControl
    {

        #region Fields and Properties

        private string _version = "V1.1";

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableToolStripButton {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether the control uses theme colors.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseThemeColors
        {
            get => ThemeButton.UseThemeColors;
            set { ThemeButton.UseThemeColors = value; Invalidate(); }
        }

        #region Image Handling
        /// <summary>
        /// Gets or sets the tinted image displayed by the control.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image TintedImage
        {
            get => ThemeButton.TintedImage;
            set { ThemeButton.TintedImage = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the accent color should be used for the tinted image.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseAccentForTintedImage
        {
            get => ThemeButton.UseAccentForTintedImage;
            set { ThemeButton.UseAccentForTintedImage = value; Invalidate(); }
        }

        #endregion Image Handling

        /// <summary>
        /// Gets or sets the color of the border for the theme button.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("BazthalLib - Appearance")]
        public Color BorderColor
        {
            get => ThemeButton.FlatAppearance.BorderColor;
            set { ThemeButton.FlatAppearance.BorderColor = value; Invalidate(); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled for the theme button.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool EnableBorder
        {
            get => ThemeButton.EnableBorder;
            set { ThemeButton.EnableBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the corners of the button are rounded.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool RoundCorners
        {
            get => ThemeButton.RoundCorners;
            set { ThemeButton.RoundCorners = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the corner radius of the button.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CornerRadius
        {
            get => ThemeButton.CornerRadius;
            set { ThemeButton.CornerRadius = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool AllowFocus
        {
            get => ThemeButton.AllowFocus;
            set { ThemeButton.AllowFocus = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a tight focus border is used for the button.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseTightFocusBorder
        {
            get => ThemeButton.UseTightFocusBorder;
            set { ThemeButton.UseTightFocusBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether focus navigation wraps around the image.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool FocusWrapAroundImage
        {
            get => ThemeButton.FocusWrapAroundImage;
            set { ThemeButton.FocusWrapAroundImage = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button's size should match the size of its image.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool MatchImageSize
        {
            get => ThemeButton.MatchImageSize;
            set { ThemeButton.MatchImageSize = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the accent color used for theming the button.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color AccentColor
        {
            get => ThemeButton.AccentColor;
            set { ThemeButton.AccentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the accent color is used for the theme button.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseAccentColor
        {
            get => ThemeButton.UseAccentColor;
            set { ThemeButton.UseAccentColor = value; Invalidate(); }
        }
        /// <summary>
        /// Gets the <see cref="ThemableButton"/> associated with this control.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ThemableButton ThemeButton => Control as ThemableButton;

        #endregion Fields and Properties

        #region Contructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableToolStripButton"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets the <see cref="AutoSize"/> property to <see langword="false"/>
        /// and initializes the <see cref="Size"/> property to a default size of 22x22 pixels. The button is themed
        /// using a <see cref="ThemableButton"/>.</remarks>
        public ThemableToolStripButton() : base(new ThemableButton())
        {
            AutoSize = false;
            Size = new System.Drawing.Size(22, 22);

        }
        #endregion Constructor

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the user interface.
        /// </summary>
        /// <remarks>This method updates the appearance of UI elements to match the provided theme
        /// colors.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the color scheme to apply.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            ThemeButton.ApplyTheme(colors);
        }
        #endregion IThemableControl Implementation
    }
}
