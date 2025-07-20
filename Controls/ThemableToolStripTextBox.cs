using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BazthalLib.UI;

namespace BazthalLib.Controls
{

    [ToolboxItem(false)] //Don't want it to appear in main toolbox only the tool strip menu
    [DefaultProperty("Items")]
    [DesignerCategory("Code")]
    [DefaultEvent("TextChanged")]
    [DesignTimeVisible(true)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class ThemableToolStripTextBox : ToolStripControlHost, IThemableControl
    {
        #region Fields and Properties

        private string _version = "V1.0";

        /// <summary>
        /// Gets or sets a value indicating whether the theme colors are used for the text box appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseThemeColors
        {
            get => TextBox.UseThemeColors;
            set { TextBox.UseThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the color of the border for the text box.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => TextBox.BorderColor;
            set { TextBox.BorderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the accent color used for the text box.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color AccentColor
        {
            get => TextBox.AccentColor;
            set { TextBox.AccentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the accent border is used for the text box.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseAccentBorder
        {
            get => TextBox.UseAccentBorder;
            set { TextBox.UseAccentBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableToolStripTextBox {_version}";

        /// <summary>
        /// Gets the underlying <see cref="ThemableTextBox"/> control associated with this component.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ThemableTextBox TextBox => Control as ThemableTextBox;
        #endregion Fields and Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableToolStripTextBox"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableToolStripTextBox"/> with a fixed size
        /// and margin, ensuring consistent appearance and layout. The <see cref="TextBox"/> is initialized with a
        /// height of 29 pixels and a margin to eliminate extra spacing. The overall control size is set to 100x29
        /// pixels.</remarks>
        public ThemableToolStripTextBox() : base(new ThemableTextBox())
        {
            AutoSize = false;
            TextBox.Height = 29; // Force size at the control level too
            TextBox.Margin = new Padding(1, 2, 1, 0); // No extra spacing
            Size = new Size(100, 29);
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Returns a string representation of the ThemableToolStripTextBox, including the current text.
        /// </summary>
        /// <returns>A string that represents the ThemableToolStripTextBox, containing the text of the associated TextBox if
        /// available; otherwise, an empty string.</returns>
        public override string ToString() => $"ThemableToolStripTextBox ({TextBox?.Text})";

        /// <summary>
        /// Gets the preferred size of the ToolStrip item, constrained by the specified size.
        /// </summary>
        /// <param name="constrainingSize">The custom size constraints for the ToolStrip item. This parameter is not used in the calculation of the
        /// preferred size.</param>
        /// <returns>A <see cref="Size"/> object representing the fixed preferred size of 150 by 24 pixels.</returns>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            // Force ToolStrip to use our fixed size
            return new Size(150, 24); 
        }

        /// <summary>
        /// Subscribes to the control's events and adjusts its height to match the ToolStrip line.
        /// </summary>
        /// <remarks>This method sets the height of the specified control to 22 to ensure it aligns with
        /// the ToolStrip line.</remarks>
        /// <param name="c">The <see cref="Control"/> to subscribe to and adjust.</param>
        protected override void OnSubscribeControlEvents(Control c)
        {
            base.OnSubscribeControlEvents(c);
            c.Height = 22; // Match height to toolstrip line
        }

        /// <summary>
        /// Unsubscribes from the events of the specified control.
        /// </summary>
        /// <remarks>This method also adjusts the height of the control to match the toolstrip
        /// line.</remarks>
        /// <param name="c">The <see cref="Control"/> from which to unsubscribe events.</param>
        protected override void OnUnsubscribeControlEvents(Control c)
        {
            base.OnUnsubscribeControlEvents(c);
            c.Height = 22; // Match height to toolstrip line
        }

        #endregion Methods and Events

        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's appearance based on the provided theme colors.
        /// Ensure that the <paramref name="colors"/> parameter is not null before calling this method.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the color scheme to apply.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            TextBox.ApplyTheme(colors);
        }
        #endregion IThemableControl Implementation
    }
}