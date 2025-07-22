using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    public class ThemableScrollBar : ThemableControlBase
    {
        #region Fields and Properties
        private readonly string _version = "V1.2";
        private bool _hoverArrows = true;
        private bool _hovering = false;
        private ThemeColors _themeColors = new();

        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _largeChange = 10;
        private int _smallChange = 1;
        private int _thumbHeight = 20; // Set this according to Max and Minmun values
        private Orientation _orientation = Orientation.Vertical;

        private enum ArrowDirection { Up, Down, Left, Right }

        private const int _buttonSize = 16;

        private bool _dragging = false;
        private int _dragOffset = 0;

        private readonly ThemableScrollBarRenderer _renderer = new();

        private Rectangle UpButtonRect =>
            Orientation == Orientation.Vertical
                ? new Rectangle(0, 0, Width, _buttonSize)
                : new Rectangle(0, 0, _buttonSize, Height);

        private Rectangle DownButtonRect =>
            Orientation == Orientation.Vertical
                ? new Rectangle(0, Height - _buttonSize, Width, _buttonSize)
                : new Rectangle(Width - _buttonSize, 0, _buttonSize, Height);

        private Rectangle TrackArea =>
            Orientation == Orientation.Vertical
                ? new Rectangle(0, _buttonSize, Width, Height - (_buttonSize * 2))
                : new Rectangle(_buttonSize, 0, Width - (_buttonSize * 2), Height);

        /// <summary>
        /// Gets the unique identifier for the themable scroll bar control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string ControlID => $"ThemableScrollBar {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether arrows are hidden until hovered over.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Hide Arrows until hovered over")]
        [DefaultValue(true)]
        public bool HoverArrows
        {
            get => _hoverArrows;
            set { _hoverArrows = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the orientation of the scroll bar.
        /// </summary>
        /// <remarks>Changing the orientation will automatically adjust the dimensions of the control to
        /// match the new orientation if necessary.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Layout")]
        [Description("The scroll bar orientation.")]
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;

                    if ((_orientation == Orientation.Vertical && Width > Height) ||
                        (_orientation == Orientation.Horizontal && Height > Width))
                    {
                        Size = new Size(Height, Width);
                    }

                    EnsureValidSize();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the scrollbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Minimum value of the scrollbar.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the maximum value of the scrollbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Maximum value of the scrollbar.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the large change value of the scrollbar.
        /// </summary>
        /// <remarks>This property is typically used to define the increment for page-up or page-down
        /// operations.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Large change value of the scrollbar.")]
        [DefaultValue(10)]
        public int LargeChange
        {
            get => _largeChange;
            set { _largeChange = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets the small change value for the scrollbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Small change value of the scrollbar")]
        [DefaultValue(1)]

        public int SmallChange
        {
            get => _smallChange;
            set
            { _smallChange = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the current value of the scrollbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Value of the scrollbar.")]
        [DefaultValue(0)]

        public int Value
        {
            get => _value;
            set
            {
                int newVal = Math.Max(_minimum, Math.Min(_maximum - _largeChange, value));
                if (_value != newVal)
                {
                    _value = newVal;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? Scroll;
        public event EventHandler? ValueChanged;
        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableScrollBar"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets the control styles to optimize painting and resizing
        /// performance, and initializes the size of the scroll bar to a default width of 16 pixels and height of 100
        /// pixels.</remarks>
        public ThemableScrollBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            Size = new Size(16, 100);
            EnsureValidSize();
            Invalidate();
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Handles the painting of the custom scrollbar control.
        /// </summary>
        /// <remarks>This method sets the renderer properties and calculates the necessary rectangles for
        /// the scrollbar components based on the current orientation and value. It then uses the renderer to draw the
        /// scrollbar and its components onto the control. The base class's <see cref="Control.OnPaint"/> method is
        /// called to draw any additional borders if enabled.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //Set the renderer properties
            _renderer.BackColor = BackColor;
            _renderer.ForeColor = ForeColor;
            _renderer.BorderColor = BorderColor;
            _renderer.AccentColor = AccentColor;
            _renderer.HoverArrows = HoverArrows;
            _renderer.Hovering = _hovering;
            _renderer.DrawOuterBorder = true;

            // Calculate button and track rectangles based on orientation
            Rectangle thumbRect = _renderer.GetThumbRectangle(Orientation, TrackArea, Minimum, Maximum, Value, LargeChange);

            //Draw the scrollbar using the renderer
            _renderer.DrawScrollBar(
                e.Graphics,
                ClientRectangle,
                Orientation,
                UpButtonRect,
                DownButtonRect,
                TrackArea,
                thumbRect
                );

            //Draw ControlBase Border if enabled
            DrawBorder( e.Graphics );

        }

        /// <summary>
        /// Ensures that the control's size meets the minimum width and height requirements based on its orientation.
        /// </summary>
        /// <remarks>If the control's dimensions are smaller than the minimum required size, this method
        /// adjusts the size to meet the minimum width and height. The minimum size is determined by the control's
        /// orientation: a vertical orientation requires a minimum width of 10 and a minimum height of 50, while a
        /// horizontal orientation requires a minimum width of 50 and a minimum height of 10.</remarks>
        private void EnsureValidSize()
        {
            int minWidth = Orientation == Orientation.Vertical ? 10 : 50;
            int minHeight = Orientation == Orientation.Vertical ? 50 : 10;

            if (Width < minWidth || Height < minHeight)
            {
                SuspendLayout();
                if (Width < minWidth || Height < minHeight)
                {
                    Size = new Size(
                        Math.Max(Width, minWidth),
                        Math.Max(Height, minHeight)
                    );
                }
                ResumeLayout();
            }
        }

        /// <summary>
        /// Handles the resize event of the control, ensuring that the control's dimensions do not fall below a logical
        /// minimum size based on its orientation.
        /// </summary>
        /// <remarks>The minimum width and height are determined by the control's orientation. For a
        /// vertical orientation, the minimum width is 10 and the minimum height is 50. For a horizontal orientation,
        /// the minimum width is 50 and the minimum height is 10. If the control's dimensions are smaller than these
        /// minimums, they are adjusted to meet the minimum size requirements.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// Handles the mouse down event for the control, adjusting the value based on the location of the click.
        /// </summary>
        /// <remarks>If the mouse click occurs within the up or down button areas, the value is adjusted
        /// by the large change amount. If the click is within the thumb area, dragging is initiated.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {

            if (UpButtonRect.Contains(e.Location))
            {
                Value -= LargeChange;
            }
            else if (DownButtonRect.Contains(e.Location))
            {
                Value += LargeChange;
            }
            else if (_renderer.GetThumbRectangle(Orientation, TrackArea, Minimum, Maximum, Value, LargeChange).Contains(e.Location))
            {
                _dragging = true;
                _dragOffset = Orientation == Orientation.Vertical
                    ? e.Y - _renderer.GetThumbRectangle(Orientation, TrackArea, Minimum, Maximum, Value, LargeChange).Top
                    : e.X - _renderer.GetThumbRectangle(Orientation, TrackArea, Minimum, Maximum, Value, LargeChange).Left;
            }
        }

        /// <summary>
        /// Handles the mouse move event to update the position of the scrollbar thumb during a drag operation.
        /// </summary>
        /// <remarks>This method updates the scrollbar's value based on the mouse position when the
        /// scrollbar thumb is being dragged. It calculates the new position relative to the track area and adjusts the
        /// scrollbar value accordingly.</remarks>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                float trackSize = Orientation == Orientation.Vertical ? TrackArea.Height : TrackArea.Width;
                int relative = Orientation == Orientation.Vertical
                    ? e.Y - TrackArea.Top - _dragOffset
                    : e.X - TrackArea.Left - _dragOffset;

                relative = Math.Max(0, Math.Min(relative, (int)trackSize - _largeChange));
                float percent = (float)relative / (trackSize - _largeChange);
                Value = _minimum + (int)(percent * (_maximum - _minimum - _largeChange));
            }

        }

        /// <summary>
        /// Handles the event when the mouse pointer enters the control's client area.
        /// </summary>
        /// <remarks>This method sets the control's hover state and triggers a redraw to visually indicate
        /// the hover effect.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovering = true;
            Invalidate(); // Redraw to show hover state
        }

        /// <summary>
        /// Handles the event when the mouse pointer leaves the control.
        /// </summary>
        /// <remarks>This method resets the hover state of the control and triggers a redraw to update the
        /// visual appearance.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovering = false;
            Invalidate(); // Redraw to show hover state
        }

        /// <summary>
        /// Handles the mouse button release event.
        /// </summary>
        /// <remarks>This method is called when a mouse button is released while the pointer is over the
        /// control. It sets the dragging state to false, indicating that a drag operation has ended.</remarks>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _dragging = false;
        }

        /// <summary>
        /// Handles the mouse wheel event to adjust the current value.
        /// </summary>
        /// <remarks>This method decreases the value by <see cref="SmallChange"/> if the mouse wheel is
        /// scrolled down, and increases it by <see cref="SmallChange"/> if scrolled up. Override this method to
        /// customize the behavior when the mouse wheel is used.</remarks>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Value -= Math.Sign(e.Delta) * SmallChange;
        }

        #endregion Methods and Events

    }

}