using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{ 
    [ToolboxItem(true)]
    public class ThemableProgressBar : ThemableControlBase
    {
        #region Fields and Properties
        private int _value = 0;
        private int _maximum = 100;
        private int _minimum = 0;
        private bool _showPercentage = false;
        private Timer _marqueeTimer;
        private int _marqueePosition = 0;
        private int _marqueeBlockWidth = 100;
        private int _marqueeSpeed = 4;
        private ProgressBarStyle _style = ProgressBarStyle.Blocks;

        private string _version = "V1.2";

        /// <summary>
        /// Gets the unique identifier for the ThemableToolStripProgressBar control.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public new string ControlID => $"ThemableToolStripProgressBar {_version}";

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the current value, constrained between 0 and the specified maximum.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(0, Math.Min(value, Maximum));
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowable value.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(1, value);
                Invalidate();
            }
        }
        /// <summary>
        /// Gets or sets the minimum allowable value.
        /// </summary>
        /// <remarks>Setting this property to a value greater than <see cref="Maximum"/> will
        /// automatically adjust it to be equal to <see cref="Maximum"/>.</remarks>
        [Category("Behavior")]
        [DefaultValue(0)]
        public int Minimum
        { 
            get => _minimum;
            set
            {
                _minimum = Math.Min(value, Maximum);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the style of the progress bar.
        /// </summary>
        /// <remarks>Changing the style to <see cref="ProgressBarStyle.Marquee"/> will start the marquee
        /// animation.</remarks>
        [Category("Appearance")]
        [DefaultValue(ProgressBarStyle.Blocks)]
        public ProgressBarStyle Style
        {
            get => _style;
            set
            {
                if (_style != value)
                {
                    _style = value;
                    _marqueePosition = -_marqueeBlockWidth;

                    if (_style == ProgressBarStyle.Marquee)
                        _marqueeTimer.Start();
                    else
                        _marqueeTimer.Stop();

                    Invalidate();
                }
            }

        }

        /// <summary>
        /// Gets or sets the speed of the marquee animation.
        /// </summary>
        /// <remarks>Adjusting the marquee speed affects how quickly the content scrolls. A higher value
        /// results in faster scrolling.</remarks>
        [Category("BazthalLib - Appearance")]
        [DefaultValue(4)]
        public int MarqueeSpeed
        {
            get => _marqueeSpeed;
            set
            {
                _marqueeSpeed = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the width of the marquee block.
        /// </summary>
        [Category("BazthalLib - Appearance")]
        [DefaultValue(100)]
        public int MarqueeWidth
        {
            get => _marqueeBlockWidth;
            set { _marqueeBlockWidth = value;  }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the percentage text is displayed in the bar.
        /// </summary>
        [Category("BazthalLib - Appearance")]
        [Description("Whether to show the percentage text in the bar.")]
        [DefaultValue (false)]
        public bool ShowPercentage
        {
            get => _showPercentage;
            set
            {
                _showPercentage = value;
                Invalidate();
            }
        }
        #endregion Fields and Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableProgressBar"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets up the progress bar with optimized double buffering and custom
        /// painting styles for smoother rendering. It also initializes a timer for marquee animation and sets default
        /// visual properties such as corner rounding and height.</remarks>
        public ThemableProgressBar()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint, true);
            this.UpdateStyles();

            _marqueeTimer = new Timer
            {
                Interval = 30 // Adjust for smoother/faster animation
            };
            _marqueeTimer.Tick += (s, e) =>
            {
                _marqueePosition += _marqueeSpeed;
                if (_marqueePosition > this.Width)
                    _marqueePosition = -_marqueeBlockWidth;
                this.Invalidate();
            };

            this.TextRenderMode = TextRenderMode.None;
            this.RoundCorners = true;
            this.CornerRadius = 6;
            this.Height = 24;
        }

        #endregion Constructor

        #region Methods and Events

        /// <summary>
        /// Handles the painting of the control, rendering the background, progress fill, and optional percentage text.
        /// </summary>
        /// <remarks>This method customizes the appearance of the progress bar by drawing the background,
        /// progress fill, and border. It supports different styles such as continuous, blocks, and marquee, and can
        /// optionally display the progress percentage as text. The method uses anti-aliasing for smoother
        /// graphics.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            // Background
            using var backBrush = new SolidBrush(this.BackColor);
            e.Graphics.FillRectangle(backBrush, bounds);

            // Progress fill
            if (Value > 0)
            {
                float percent = (float)(Value - Minimum) / (Maximum - Minimum);
                int fillWidth = (int)((Width - 2) * percent);
                Rectangle fillRect = new Rectangle(1, 1, fillWidth, Height - 2);

                using Brush fillBrush = new SolidBrush(this.AccentColor);

                switch (_style)
                {
                    case ProgressBarStyle.Continuous:
                        using (GraphicsPath path = RoundCorners
                            ? CreateRoundedPath(fillRect, CornerRadius)
                            : new GraphicsPath())
                        {
                            if (!RoundCorners) path.AddRectangle(fillRect);
                            e.Graphics.FillPath(fillBrush, path);
                        }
                        break;

                    case ProgressBarStyle.Blocks:
                        int blockWidth = 6;
                        int gap = 2;
                        for (int x = 1; x < fillWidth; x += blockWidth + gap)
                        {
                            Rectangle block = new Rectangle(x, 1, Math.Min(blockWidth, fillWidth - x), Height - 2);
                            e.Graphics.FillRectangle(fillBrush, block);
                        }
                        break;

                    case ProgressBarStyle.Marquee:
                        Rectangle marqueeRect = new Rectangle(_marqueePosition, 1, _marqueeBlockWidth, Height - 2);

                        using (Brush marqueeBrush = new SolidBrush(this.AccentColor))
                        {
                            using GraphicsPath path = RoundCorners
                                ? CreateRoundedPath(marqueeRect, CornerRadius)
                                : new GraphicsPath();

                            if (!RoundCorners)
                                path.AddRectangle(marqueeRect);

                            e.Graphics.FillPath(marqueeBrush, path);
                        }
                        break;

                }
            }

            if (ShowPercentage)
            {
                string percentText = $"{(int)((float)Value / Maximum * 100)}%";
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using var textBrush = new SolidBrush(this.ForeColor);
                e.Graphics.DrawString(percentText, this.Font, textBrush, bounds, sf);
            }

            // Border
            DrawBorder(e.Graphics);
        }

        /// <summary>
        /// Creates a <see cref="GraphicsPath"/> representing a rectangle with rounded corners.
        /// </summary>
        /// <remarks>The method constructs a path by adding arcs to each corner of the specified
        /// rectangle, creating a smooth rounded effect.</remarks>
        /// <param name="bounds">The <see cref="Rectangle"/> that defines the bounds of the rounded rectangle.</param>
        /// <param name="radius">The radius of the corners. Must be a non-negative integer.</param>
        /// <returns>A <see cref="GraphicsPath"/> object that represents the rounded rectangle.</returns>
        private GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the component and optionally releases the managed resources.
        /// </summary>
        /// <remarks>This method is called by the public <c>Dispose</c> method and the finalizer. When
        /// <paramref name="disposing"/> is <see langword="true"/>, it releases all resources held by any managed
        /// objects that this component references. This method should be overridden by derived classes to release
        /// resources specific to those classes.</remarks>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _marqueeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }


        #endregion Methods and Events
    }

}
