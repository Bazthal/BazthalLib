using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;


namespace BazthalLib.Controls
{
    public class ThemableLabel : Label, IThemableControl
    {
        #region Fields
        private string _version = "V1.0.1";
        private Color _borderColor = Color.Gray;
        //private Color _accentColor = Color.DodgerBlue;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();
        //private bool _useAccentColor = false;
        private bool _enableBorder = false;


        #endregion Fields

        #region Properties


        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableLabel {_version}";

        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use theme colors or not.")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the border.")]
        [DefaultValue(true)]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /*
        // Not Yet Used so has been removed For now
        
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
        */
        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to enable the border or not.")]
        [DefaultValue(false)]
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
            set
            {
                base.BorderStyle = BorderStyle.None;
            }
        }

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableLabel"/> class.
        /// </summary>
        /// <remarks>This constructor sets the control styles to optimize double buffering and redraw the
        /// control when resized.</remarks>
        public ThemableLabel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Draws a custom border around the control.
        /// </summary>
        /// <remarks>If the control is in design mode, a dashed border is drawn for preview purposes.
        /// Otherwise, a solid border is drawn if the border is enabled. The control's region is set to the border
        /// rectangle.</remarks>
        /// <param name="e">The <see cref="PaintEventArgs"/> containing the event data for the paint operation.</param>
        private void DrawCustomBorder(PaintEventArgs e)
        {
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
                    using Pen pen = new Pen(_borderColor, 2);
                    if (_enableBorder)
                        e.Graphics.DrawRectangle(pen, rect); // Draw border with sharp corners if enabled

                    Region = new Region(rect); // Set the region to the rectangle to clip the button with / without border
                }

            }

        }

        #endregion Methods

        #region Events
        #endregion Events

        #region Overrides
        /// <summary>
        /// Handles the painting of the control, including drawing a custom border if enabled.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="Control.OnPaint"/> method to provide custom
        /// painting logic. If the border is enabled, it draws a custom border around the control.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data for the paint operation.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_enableBorder)
            {
                DrawCustomBorder(e);
            }
        }

        /// <summary>
        /// Handles the resize event of the control.
        /// </summary>
        /// <remarks>This method invalidates the control to ensure it is redrawn when its size
        /// changes.</remarks>
        /// <param name="eventargs">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate(); // Invalidate to redraw the control when resized
        }

        #endregion Overrides

        #region  IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's background, foreground, and border colors based on
        /// the provided theme. If theming is disabled or the <paramref name="colors"/> parameter is null, the method
        /// logs a message and returns without making changes.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {

            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableLabel", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableLabel", "Theming is disabled.");
                return;
            }
            _themeColors = colors; //Store here to allow ThemColor to not be null
            BackColor = colors.BackColor;
            ForeColor = colors.ForeColor;
            _borderColor = colors.BorderColor;
            //           _accentColor = colors.AccentColor;
            Invalidate(); // Force a redraw to apply the new theme colors
        }
        #endregion IThemableControl Implementation




    }


}
