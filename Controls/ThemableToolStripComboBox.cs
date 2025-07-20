using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BazthalLib.Controls
{

    [ToolboxItem(false)] //Don't want it to appear in main toolbox only the tool strip menu
    [DefaultProperty("Items")]
    [DesignerCategory("Code")]
    [DefaultEvent("SelectedIndexChanged")]
    [DesignTimeVisible(true)]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.MenuStrip)]

    public class ThemableToolStripComboBox : ToolStripControlHost
    {
        #region Fields and Properties

        private string _version = "V1.1";

        /// <summary>
        /// Gets the underlying <see cref="ThemableComboBox"/> control associated with this component.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ThemableComboBox ComboBox => Control as ThemableComboBox;

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableToolStripComboBox {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether the ComboBox should use theme colors.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseThemeColors
        {
            get => ComboBox.UseThemeColors;
            set { ComboBox.UseThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the color of the border for the ComboBox.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => ComboBox.BorderColor;
            set { ComboBox.BorderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the accent color used for the ComboBox control.
        /// </summary>
        /// <remarks>Changing this property will update the visual appearance of the ComboBox to reflect
        /// the new accent color.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color AccentColor
        {
            get => ComboBox.AccentColor;
            set { ComboBox.AccentColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the background color of the selected item in the ComboBox.
        /// </summary>
        /// <remarks>Changing this property will update the appearance of the selected item and trigger a
        /// repaint of the control.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SelectedItemBackColor
        {
            get => ComboBox.SelectedItemBackColor;
            set { ComboBox.SelectedItemBackColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the foreground color of the selected item in the ComboBox.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SelectedItemForeColor
        {
            get => ComboBox.SelectedItemForeColor;
            set { ComboBox.SelectedItemForeColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the height, in pixels, of each item in the combo box.
        /// </summary>
        /// <remarks>Changing the item height will cause the control to be redrawn to reflect the new
        /// size.</remarks>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("BazthalLib - Appearance")]
        public int ItemHeight
        {
            get => ComboBox.ItemHeight;
            set { ComboBox.ItemHeight = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the height of the buffer for the ComboBox control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("BazthalLib - Appearance")]
        public int BufferHeight
        {
            get => ComboBox.BufferHeight;
            set { ComboBox.BufferHeight = value; Invalidate(); }
        }

        /// <summary>
        /// Gets the collection of items displayed in the dropdown.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Data")]
        [Description("The items displayed in the dropdown.")]
        public NotifyingItemCollection Items => ComboBox.Items;

        //public ComboBox.ObjectCollection Items => ComboBox.Items;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {

            get => ComboBox.SelectedIndex;
            set
            {
                ComboBox.SelectedIndex = value;
                Invalidate();
                DebugUtils.Log("SelectedIndex", "ThemableToolStripComboBox", $"{ComboBox.SelectedIndex}");
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get => ComboBox.SelectedItem;

            set => ComboBox.SelectedItem = (string)value;
        }

        public event EventHandler SelectedIndexChanged
        {
            add => ComboBox.SelectedIndexChanged += value;
            remove => ComboBox.SelectedIndexChanged -= value;
        }

        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableToolStripComboBox"/> class with a custom combo box.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableToolStripComboBox"/> with specific
        /// dimensions and margins to ensure consistent theming and layout. The combo box is initialized with a fixed
        /// height and margin to maintain a uniform appearance.</remarks>
        public ThemableToolStripComboBox() : base(new ThemableComboBox())
        {
            AutoSize = false;
            ComboBox.Height = 22; // Force size at the control level too
            ComboBox.Margin = new Padding(1, 2, 1, 0); // No extra spacing
            Size = new Size(150, 22);
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Returns a string representation of the <see cref="ThemableToolStripComboBox"/>.
        /// </summary>
        /// <returns>A string that represents the current state of the <see cref="ThemableToolStripComboBox"/>, including the
        /// text of the associated <see cref="ComboBox"/> if available.</returns>
        public override string ToString() => $"ThemableToolStripComboBox ({ComboBox?.Text})";

        /// <summary>
        /// Gets the preferred size of the ToolStrip item, constrained by the specified size.
        /// </summary>
        /// <param name="constrainingSize">The custom size constraints to consider when calculating the preferred size.</param>
        /// <returns>A <see cref="Size"/> object representing the preferred size of the ToolStrip item, which is fixed at 150 by
        /// 24 pixels.</returns>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            // Force ToolStrip to use our fixed size
            return new Size(150, 24); // Match your item height
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
        /// <remarks>This method adjusts the height of the control to match the toolstrip line after
        /// unsubscribing from its events.</remarks>
        /// <param name="c">The <see cref="Control"/> from which to unsubscribe events.</param>
        protected override void OnUnsubscribeControlEvents(Control c)
        {
            base.OnUnsubscribeControlEvents(c);
            c.Height = 22; // Match height to toolstrip line
        }

        #endregion Methods and Events

    }
}