using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BazthalLib.UI;

namespace BazthalLib.Controls
{

    [DefaultProperty("Value")]
    [DesignerCategory("Code")]
    [DefaultEvent("ValueChanged")]
    [ToolboxItem(false)]
    [DesignTimeVisible(true)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public class ThemableToolStripProgressBar : ToolStripControlHost, IThemableControl
    {

        #region Fields and Properties
        private string _version = "V1.1";

        /// <summary>
        /// Gets the unique identifier for the ThemableToolStripProgressBar control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableToolStripProgressBar {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether the progress bar uses theme colors.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseThemeColors
        {
            get => ProgressBar.UseThemeColors;
            set { ProgressBar.UseThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the color of the border for the progress bar.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("BazthalLib - Appearance")]
        public Color BorderColor
        {
            get => ProgressBar.BorderColor;
            set { ProgressBar.BorderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled for the progress bar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool EnableBorder
        {
            get => ProgressBar.EnableBorder;
            set { ProgressBar.EnableBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the accent color used for the progress bar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color AccentColor
        {
            get => ProgressBar.AccentColor;
            set { ProgressBar.AccentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the current value of the progress bar.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Value
        {
            get => ProgressBar.Value;
            set => ProgressBar.Value = value;
        }

        /// <summary>
        /// Gets or sets the minimum value of the progress bar.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Minimum
        {
            get => ProgressBar.Minimum;
            set => ProgressBar.Minimum = value;
        }

        /// <summary>
        /// Gets or sets the maximum value of the progress bar.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Maximum
        {
            get => ProgressBar.Maximum;
            set => ProgressBar.Maximum = value;
        }

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// Gets the <see cref="ThemableProgressBar"/> associated with the control.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ThemableProgressBar ProgressBar => Control as ThemableProgressBar;
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableToolStripProgressBar"/> class with a custom progress
        /// bar.
        /// </summary>
        /// <remarks>This constructor sets up the progress bar with a fixed size and margin, ensuring
        /// consistent appearance. The <see cref="ThemableToolStripProgressBar"/> is designed to integrate with theming
        /// systems, allowing for a customizable user interface.</remarks>
        public ThemableToolStripProgressBar() : base(new ThemableProgressBar())
        {
            AutoSize = false;
            ProgressBar.Height = 16; // Force size at the control level too
            ProgressBar.Margin = new Padding(1, 4, 1, 4); // No extra spacing
            Size = new Size(100, 16);
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Gets the preferred size of the ToolStrip item, constrained by the specified size.
        /// </summary>
        /// <param name="constrainingSize">The custom size constraints for the ToolStrip item.</param>
        /// <returns>A <see cref="Size"/> object representing the preferred size of the ToolStrip item, which is fixed at 100 by
        /// 16 pixels.</returns>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            // Force ToolStrip to use our fixed size
            return new Size(100, 16); // Match your item height
        }

        /// <summary>
        /// Subscribes to the control's events and adjusts its height to match the ToolStrip line.
        /// </summary>
        /// <remarks>This method sets the height of the specified control to 16 to ensure it aligns with
        /// the ToolStrip line height.</remarks>
        /// <param name="c">The control to which events are subscribed.</param>
        protected override void OnSubscribeControlEvents(Control c)
        {
            base.OnSubscribeControlEvents(c);
            c.Height = 16; // Match height to toolstrip line
        }

        /// <summary>
        /// Unsubscribes from the events of the specified control.
        /// </summary>
        /// <remarks>This method adjusts the height of the control to match the toolstrip line after
        /// unsubscribing from its events.</remarks>
        /// <param name="c">The <see cref="Control"/> from which to unsubscribe events.</param>
        protected override void OnUnsubscribeControlEvents(Control c)
        {
            base.OnUnsubscribeControlEvents(c);
            c.Height = 16; // Match height to toolstrip line
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
            ProgressBar.ApplyTheme(colors);
        }
        #endregion IThemableControl Implementation
    }
}
