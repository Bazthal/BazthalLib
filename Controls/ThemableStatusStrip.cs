using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ThemableStatusStrip : StatusStrip, IThemableControl
    {
        #region Fields and Properties
        private readonly string _version = "V1.0";
        private bool _useThemeColors = true;
        private Color _backColor = SystemColors.Control;
        private Color _foreColor = SystemColors.ControlText;
        private Color _borderColor = SystemColors.ControlDark;

        /// <summary>
        /// Gets the unique identifier for the control, which includes the version information.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableStatusStrip {_version}";

        /// <summary>
        /// Gets or sets the color of the bottom border.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The color of the bottom border.")]
        [DefaultValue(typeof(Color), "ControlDark")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether theme colors are used for this toolstrip.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Use theme colors for this toolstrip.")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableStatusStrip"/> class with custom styling and rendering
        /// options.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableStatusStrip"/> with optimized double
        /// buffering and resize redrawing for smoother rendering. It also customizes the appearance by setting a fixed
        /// height, padding, and margin, and uses a system render mode with a custom renderer to disable
        /// borders.</remarks>
        public ThemableStatusStrip()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Height = 32; // Slightly taller than default ToolStrip
            //GripStyle = ToolStripGripStyle.Visible;
            Padding = new Padding(0, 0, 0, 3);
            Margin = new Padding(0);
            RenderMode = ToolStripRenderMode.System;

            // Disable renderer's border
            Renderer = new ToolStripProfessionalRenderer(new NoBorderColorTable());
        }
        /// <summary>
        /// Provides a color table for a ToolStrip with no border color.
        /// </summary>
        /// <remarks>This class overrides the default border color of a ToolStrip to be transparent,
        /// effectively removing the border. It is useful for creating a seamless appearance in custom UI designs where
        /// a border is not desired.</remarks>
        private class NoBorderColorTable : ProfessionalColorTable
        {
            public override Color ToolStripBorder => Color.Transparent;
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the painting of the control's borders.
        /// </summary>
        /// <remarks>This method customizes the painting of the control by drawing a top and bottom border
        /// using the specified border color. It is called automatically during the control's paint cycle.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using var pen = new Pen(_borderColor, 2);
            // Draw cutom top border
            e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);

            // Draw custom bottom border
            e.Graphics.DrawLine(pen, 0, 0, Width, 0);
        }

        #endregion Methods and Events


        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control and its child items.
        /// </summary>
        /// <remarks>This method updates the background, foreground, and border colors of the control and
        /// its child items based on the provided theme colors. If the control hosts other themable controls, it
        /// recursively applies the theme to them as well. If <paramref name="colors"/> is null or theme colors are not
        /// in use, the method returns without making changes.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!_useThemeColors || colors == null)
                return;

            _backColor = colors.BackColor;
            _foreColor = colors.ForeColor;
            _borderColor = colors.BorderColor;
            BackColor = _backColor;
            ForeColor = _foreColor;

            foreach (ToolStripItem item in Items)
            {
                if (item is ToolStripControlHost host)
                {
                    if (host.Control is IThemableControl themable)
                    {
                        themable.ApplyTheme(colors);
                    }
                    else
                    {
                        host.Control.BackColor = colors.BackColor;
                        host.Control.ForeColor = colors.ForeColor;
                    }
                }
                else if (item is IThemableControl themable)
                {
                    themable.ApplyTheme(colors);
                }
                else
                {
                    item.BackColor = colors.BackColor;
                    item.ForeColor = colors.ForeColor;
                }
            }

            Invalidate();
        }
        #endregion IThemableControl Implementation
    }
}
