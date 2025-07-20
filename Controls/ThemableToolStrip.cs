using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ThemableToolStrip : ToolStrip, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private bool _useThemeColors = true;
        private Color _backColor = SystemColors.Control;
        private Color _foreColor = SystemColors.ControlText;
        private Color _borderColor = SystemColors.ControlDark;

        /// <summary>
        /// Gets the unique identifier for the ThemableToolStrip control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableToolStrip {_version}";

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
        /// Initializes a new instance of the <see cref="ThemableToolStrip"/> class with optimized rendering settings.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableToolStrip"/> with specific styles and
        /// dimensions to enhance performance and appearance. It disables automatic resizing, sets a custom height, and
        /// applies padding and margin settings. The rendering mode is set to system, and a custom renderer is used to
        /// eliminate borders.</remarks>
        public ThemableToolStrip()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Height = 32; // Slightly taller than default ToolStrip
            //GripStyle = ToolStripGripStyle.Visible;
            Padding = new Padding(3, 3, 3, 3);
            Margin = new Padding(0);
            RenderMode = ToolStripRenderMode.System;

            // Disable renderer's border
            Renderer = new ToolStripProfessionalRenderer(new NoBorderColorTable());
        }
        private class NoBorderColorTable : ProfessionalColorTable
        {
            public override Color ToolStripBorder => Color.Transparent;
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the painting of the control's border.
        /// </summary>
        /// <remarks>This method draws a custom border around the control using the specified border
        /// color. It overrides the base <see cref="Control.OnPaint"/> method to provide custom painting
        /// logic.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using Pen pen = new Pen(BorderColor, 4);
            e.Graphics.DrawRectangle(pen, rect);
            Region = new Region(rect);
            // Draw custom bottom border
            //  using var pen = new Pen(_borderColor, 1);
            //   e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }

        #endregion Methods and Events


        #region IThemableControl Implementation
        /// <summary>
        /// Applies the specified theme colors to the control and its child items.
        /// </summary>
        /// <remarks>This method updates the background, foreground, and border colors of the control
        /// based on the provided theme colors. It also recursively applies the theme to any child controls that
        /// implement the <see cref="IThemableControl"/> interface. If the control does not use theme colors or if the
        /// <paramref name="colors"/> parameter is null, the method returns without making changes.</remarks>
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
