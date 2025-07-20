 using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    public class ThemableTabControlBase : TabControl, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private Color _borderColor = Color.Gray;
        private Color _pageBackColor = SystemColors.Control;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();
        private bool _enableBorder = true;
        private bool _enableDesignerTabHeader = false;

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableTabControlBase {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether a border is drawn around the tab control.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to draw a border around the tab control.")]
        [DefaultValue(true)]
        public bool EnableBorder
        {
            get => _enableBorder;
            set { _enableBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the default tab header is shown in the designer.
        /// </summary>
        /// <remarks>This property is intended for use in the designer only. It should be disabled when
        /// not needed, as it allows page switching in the designer by using the built-in tab header, which supports
        /// click events by default.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to show the default tab header in the designer - This is only visible in the designer \n Should be disabled when not using it")]
        [DefaultValue(false)]        
        public bool EnableDesignerTabHeader
        {
            get => _enableDesignerTabHeader;
            set
            {
                _enableDesignerTabHeader = value;
                if (DesignMode)
                {
                    SizeMode = value ? TabSizeMode.Normal : TabSizeMode.Fixed;

                    DrawMode = value ? TabDrawMode.Normal : TabDrawMode.OwnerDrawFixed;
                    Appearance = value ? TabAppearance.Normal : TabAppearance.FlatButtons;
                    ItemSize = value ? new Size(0, 20) : new Size(0, 1); // Push tabs off screen
                    foreach (TabPage tab in TabPages)
                    {
                        tab.UseVisualStyleBackColor = value ? false : true; // Prevents added controls from having messed up colors 
                    }
                }

                Invalidate(); // Force redraw to apply changes
            }
        }

        /// <summary>
        /// Gets or sets the color of the border around the tab control.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the border around the tab control.")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the background color of the tab pages.
        /// </summary>
        /// <remarks>Changing this property updates the background color of all tab pages and triggers a
        /// repaint of the control.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The background color of the tab pages.")]
        [DefaultValue(typeof(Color), "Control")]
        public Color PageBackColor
        {
            get => _pageBackColor;
            set
            {
                _pageBackColor = value;
                foreach (TabPage tab in TabPages)
                {
                    tab.BackColor = _pageBackColor;
                }
                Invalidate();
            }
        }
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

        #endregion Fields and Properties


        #region Methods and Events

        /// <summary>
        /// Handles the creation of the control's handle and applies specific styles and settings.
        /// </summary>
        /// <remarks>This method is overridden to customize the control's appearance and behavior when the
        /// handle is created. If the control is not in design mode, it sets the tab height to 1 pixel to hide the tabs
        /// at runtime and enables double buffering for optimized painting.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                ItemSize = new Size(0, 1);//Make the tab height to 1 pixel to hide the tabs at runtime
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }

        }

        /// <summary>
        /// Paints the control's border if the border is enabled.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="Control.OnPaint"/> method to draw a border
        /// around the control when the border is enabled. The border is drawn using the specified border color and a
        /// fixed width.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_enableBorder)
                return;
            using Pen borderPen = new Pen(_borderColor, 2);

            Rectangle contentRect = ClientRectangle;
            contentRect.Inflate(-1, -1);

            e.Graphics.DrawRectangle(borderPen, contentRect);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ListBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <remarks>This method is called whenever the selected index changes in the list box.  It allows
        /// derived classes to handle the event without attaching a delegate.</remarks>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
        }

        /// <summary>
        /// Paints the background of the control with the specified color.
        /// </summary>
        /// <remarks>This method clears the background using the control's designated background color. It does not
        /// perform any action if the control is in design mode.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (DesignMode) return;
            e.Graphics.Clear(_pageBackColor);
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
        /// Applies the specified theme colors to the control and its tab pages.
        /// </summary>
        /// <remarks>This method updates the control's border, background, and foreground colors based on
        /// the provided theme colors. If theming is disabled or the <paramref name="colors"/> parameter is <see
        /// langword="null"/>, the method will log a message and return without making changes.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be <see langword="null"/>.</param>
        public void ApplyTheme(ThemeColors colors)
        {

            if (!UseThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableTabContolBase", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableTabControlBase", "Theming is disabled");
                return;
            }

            _themeColors = colors;
            BorderColor = Enabled ? colors.BorderColor : colors.DisabledColor;
            PageBackColor = colors.BackColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            foreach (TabPage tab in TabPages)
            {
                tab.BackColor = colors.BackColor;
                tab.ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            }

            Invalidate(); // Force redraw
        }
        #endregion IThemableControl Implementation
    }
}

