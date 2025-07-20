using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    [DefaultEvent("ValueChanged")]
    public class ThemableNumericUpDown : UserControl, IThemableControl
    {

        #region Fields and Properties
        private string _version = "V1.1";
        private ThemableTextBox numericUpDown;
        private Panel upButton;
        private Panel downButton;
        private decimal _value = 0;
        private int _minimum = 0;
        private int _maximum = 100;

        private Color _accentColor = Color.DodgerBlue;
        private Color _borderColor = Color.Gray;
        private ThemeColors _themeColors = new();
        private bool _useThemeColors = true;


        public event EventHandler ValueChanged;

        /// <summary>
        /// Gets or sets the current numeric value.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("The current numeric value.")]
        [DefaultValue(0)]
        public decimal Value
        {
            get => _value;
            set
            {
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                numericUpDown.Text = _value.ToString();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the unique identifier for the control, incorporating the current version.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableNumericUpDown {_version}";

        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("The minimum allowed value.")]
        [DefaultValue(0)]
        public int Minimum { get => _minimum; set => _minimum = value; }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("The maximum allowed value.")]
        [DefaultValue(100)]
        public int Maximum { get => _maximum; set => _maximum = value; }

        /// <summary>
        /// Gets or sets a value indicating whether theme colors are used for the appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets the accent color used for highlighting elements in the UI.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableNumericUpDown"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the control with default styling and layout. It initializes
        /// the numeric input area and the up and down buttons, arranging them within the control. The control is
        /// double-buffered to reduce flicker during resizing and redrawing.</remarks>
        public ThemableNumericUpDown()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Padding = new Padding(1);

            this.Size = new Size(100, 25);
            numericUpDown = new ThemableTextBox
            {
                TextAlign = HorizontalAlignment.Left,
                Dock = DockStyle.Fill,
                Multiline = true,
                Margin = new Padding(0),
                Text = _value.ToString(),
            };

            numericUpDown.KeyPress += TextBox_KeyPress;

            upButton = new Panel
            {
                Width = 18,
                Dock = DockStyle.Right,
                Height = numericUpDown.Height / 2,
                Cursor = Cursors.Hand
            };

            downButton = new Panel
            {
                Width = 18,
                Dock = DockStyle.Right,
                Height = numericUpDown.Height / 2,
                Cursor = Cursors.Hand
            };

            upButton.Paint += (s, e) => DrawArrow(e.Graphics, upButton.ClientRectangle, true);
            downButton.Paint += (s, e) => DrawArrow(e.Graphics, downButton.ClientRectangle, false);

            upButton.Click += (s, e) => Value++;
            downButton.Click += (s, e) => Value--;

            // Add a container panel to stack buttons vertically
            Panel buttonPanel = new Panel
            {
                Width = 18,
                Dock = DockStyle.Right
            };

            buttonPanel.Controls.Add(downButton);
            buttonPanel.Controls.Add(upButton);
            upButton.Dock = DockStyle.Top;
            downButton.Dock = DockStyle.Bottom;

            Controls.Add(numericUpDown);
            Controls.Add(buttonPanel);
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the <see cref="ThemableTextBox"/> key press event to restrict input to digits only and prevent new
        /// lines.
        /// </summary>
        /// <remarks>This method blocks the Enter/Return key to prevent new lines and ensures that only
        /// numeric input is allowed. If the Enter key is pressed, it attempts to parse the current text as a decimal
        /// and updates the value within the specified range. If parsing fails, it resets the text to the last valid
        /// value.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ThemableTextBox"/>.</param>
        /// <param name="e">A <see cref="KeyPressEventArgs"/> that contains the event data.</param>
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Block Enter/Return key to avoid new lines
            if (e.KeyChar == (char)Keys.Return)
            {
                if (sender is ThemableTextBox textBox)
                {
                    if (decimal.TryParse(textBox.Text, out decimal value))
                    {
                        Value = Math.Max(_minimum, Math.Min(_maximum, value));
                    }
                    else
                    {
                        // If parsing fails, reset to the last valid value
                        textBox.Text = Value.ToString();
                    }
                    textBox.Text = Value.ToString();
                }
                e.Handled = true;
                return;
            }

            // Allow control keys like Backspace
            if (char.IsControl(e.KeyChar))
                return;

            // Allow digits only
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Paints the border of the control using the specified graphics context.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="Control.OnPaint"/> method to draw a border
        /// around the control. The border is drawn using the current <see cref="BorderColor"/>.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Nothing is drawn on the base control, only border gets added
            base.OnPaint(e);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        }

        /// <summary>
        /// Draws an arrow within the specified rectangle on the provided graphics surface.
        /// </summary>
        /// <remarks>The arrow is centered within the specified rectangle. The color of the arrow is
        /// determined by the <c>AccentColor</c> property.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object on which to draw the arrow.</param>
        /// <param name="rect">The <see cref="Rectangle"/> that defines the bounds within which the arrow is drawn.</param>
        /// <param name="up"><see langword="true"/> to draw an upward-pointing arrow; <see langword="false"/> to draw a downward-pointing
        /// arrow.</param>
        private void DrawArrow(Graphics g, Rectangle rect, bool up)
        {
            Point center = new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            Point[] arrow = up
                ? new[] {
                new Point(center.X - 4, center.Y + 2),
                new Point(center.X + 4, center.Y + 2),
                new Point(center.X, center.Y - 3)
                  }
                : new[] {
                new Point(center.X - 4, center.Y - 2),
                new Point(center.X + 4, center.Y - 2),
                new Point(center.X, center.Y + 3)
                  };

            using var brush = new SolidBrush(AccentColor);
            g.FillPolygon(brush, arrow);
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
        /// Applies the specified theme colors to the control.
        /// </summary>
        /// <remarks>This method updates the control's background, foreground, accent, and border colors
        /// based on the provided theme colors. If the control is disabled, the disabled color from the theme is used
        /// for the foreground, accent, and border colors. The method does nothing if <paramref name="colors"/> is null
        /// or if theme colors are not being used.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {
            if (!UseThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableNumericUpDown", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableNumericUpDown", "Skipping theme assignment");
                return;
            }
            _themeColors = colors;

            BackColor = colors.BackColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            _accentColor = Enabled ? colors.AccentColor : colors.DisabledColor;
            _borderColor = Enabled ? colors.BorderColor : colors.DisabledColor;

            Invalidate(); // Force redraw
        }

        #endregion IThemableControl Implementation

    }

}
