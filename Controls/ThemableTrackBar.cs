using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    [DefaultEvent("Scroll")]
    public class ThemableTrackBar : ThemableControlBase
    {
        #region Fields
        private readonly string _version = "V1.2";
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;

        private int _thumbSize = 8;
        private int _trackThickness = 4;
        private int _margin = 4;

        private bool _useProgressFill = true;
        private bool _roundedThumb = true;
        private Orientation _orientation = Orientation.Horizontal;

        private bool _dragging = false;


        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier for the ThemableTrackBar control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string ControlID => $"ThemableTrackBar {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new bool EnableBorder { get => base.EnableBorder; set => base.EnableBorder = value; }

        /// <summary>
        /// Gets or sets the minimum value of the trackbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Minimum value of the trackbar.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets the maximum value of the trackbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Maximum value of the trackbar.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the current value of the trackbar.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Value of the trackbar.")]
        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                int clamped = Math.Clamp(value, _minimum, _maximum);
                if (_value != clamped)
                {
                    _value = clamped;
                    Invalidate();
                    OnValueChanged(EventArgs.Empty);
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the thumb in pixels.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Size of the thumb in pixels.")]
        [DefaultValue(8)]
        public int ThumbSize
        {
            get => _thumbSize;
            set { _thumbSize = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the thickness of the track in pixels.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Thickness of the track in pixels.")]
        [DefaultValue(4)]
        public int TrackThickness
        {
            get => _trackThickness;
            set { _trackThickness = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the margin around the track in pixels.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Margin around the track in pixels.")]
        [DefaultValue(4)]
        public int TrackMargin
        {
            get => _margin;
            set { _margin = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the orientation of the control, which determines the layout direction.
        /// </summary>
        /// <remarks>Changing the orientation may automatically adjust the control's size to maintain a
        /// valid layout.</remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get => _orientation;
            set
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
        /// Gets or sets a value indicating whether the progress fill is used in the control's appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DefaultValue(true)]
        public bool UseProgressFill
        {
            get => _useProgressFill;
            set { _useProgressFill = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thumb of the control is displayed with rounded edges.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [DefaultValue(true)]
        public bool RoundedThumb
        {
            get => _roundedThumb;
            set { _roundedThumb = value; Invalidate(); }
        }

        /// <summary>
        /// Gets a value indicating whether a drag operation is currently in progress.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Dragging { get => _dragging; }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableTrackBar"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets the control styles to optimize painting and ensure smooth
        /// resizing and redrawing. It also initializes the size of the track bar to a default value and validates the
        /// size to ensure it is within acceptable limits.</remarks>
        public ThemableTrackBar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);

            this.Size = new Size(150, 16);
            EnsureValidSize();
            Invalidate();
        }

#endregion Conttructor

        #region Paint

        /// <summary>
        /// Renders the control's visual elements, including the track, thumb, and optional progress fill.
        /// </summary>
        /// <remarks>This method customizes the painting of the control by drawing a focus rectangle if
        /// the control is focused, and rendering the track and thumb based on the current value and orientation. The
        /// method supports both horizontal and vertical orientations and applies anti-aliasing for smoother
        /// graphics.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Focused) { ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(2, 2, Width - 4, Height - 4)); }
            e.Graphics.SetClip(ClientRectangle);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float percent = (float)(_value - _minimum) / (_maximum - _minimum);

            if (_orientation == Orientation.Horizontal)
            {
                int y = Height / 2;
                int left = _margin;
                int right = Width - _margin;
                int trackWidth = right - left;

                int thumbX = left + (int)(percent * trackWidth);
                thumbX = Math.Clamp(thumbX, left, right);
                // Draw track
                using var railBrush = new SolidBrush(BorderColor);
                e.Graphics.FillRectangle(railBrush, left, y - _trackThickness / 2, trackWidth, _trackThickness);

                // Fill trail
                if (_useProgressFill)
                {
                    using var fillBrush = new SolidBrush(AccentColor);
                    e.Graphics.FillRectangle(fillBrush, left, y - _trackThickness / 2, thumbX - left, _trackThickness);
                }

                // Thumb
                using var thumbBrush = new SolidBrush(AccentColor);
                if (_roundedThumb)
                    e.Graphics.FillEllipse(thumbBrush, thumbX - _thumbSize / 2, y - _thumbSize / 2, _thumbSize, _thumbSize);
                else
                    e.Graphics.FillRectangle(thumbBrush, thumbX - _thumbSize / 2, y - _thumbSize / 2, _thumbSize / 2, _thumbSize);
            }
            else // Vertical
            {
                int x = Width / 2;
                int top = _margin;
                int bottom = Height - _margin;
                int trackHeight = bottom - top;

                int thumbY = bottom - (int)(percent * trackHeight);
                thumbY = Math.Clamp(thumbY, top, bottom);


                // Draw track
                using var railBrush = new SolidBrush(BorderColor);
                e.Graphics.FillRectangle(railBrush, x - _trackThickness / 2, top, _trackThickness, trackHeight);

                // Fill trail
                if (_useProgressFill)
                {
                    using var fillBrush = new SolidBrush(AccentColor);
                    e.Graphics.FillRectangle(fillBrush, x - _trackThickness / 2, thumbY, _trackThickness, bottom - thumbY);
                }

                // Thumb
                using var thumbBrush = new SolidBrush(AccentColor);
                if (_roundedThumb)
                    e.Graphics.FillEllipse(thumbBrush, x - _thumbSize / 2, thumbY - _thumbSize / 2, _thumbSize, _thumbSize);
                else
                    e.Graphics.FillRectangle(thumbBrush, x - _thumbSize / 2, thumbY - _thumbSize / 2, _thumbSize, _thumbSize);
            }


        }

        #endregion

        #region Mouse Interaction
        /// <summary>
        /// Handles the mouse down event for the control.
        /// </summary>
        /// <remarks>This method sets the control into a dragging state and focuses it. It also updates
        /// the control's value based on the mouse location.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _dragging = true;
            Focus();
            SetValueFromMouse(e.Location);
        }

        /// <summary>
        /// Handles the mouse move event for the control.
        /// </summary>
        /// <remarks>This method updates the control's value based on the mouse position if a drag
        /// operation is in progress.</remarks>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data, including the current mouse location.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
                SetValueFromMouse(e.Location);
        }

        /// <summary>
        /// Handles the mouse button release event for the control.
        /// </summary>
        /// <remarks>This method stops the dragging operation if it is in progress and triggers the scroll
        /// completion event.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_dragging)
            {
                _dragging = false;
                OnScrollCompleted();
            }
        }

        /// <summary>
        /// Handles the mouse wheel event to adjust the value based on the scroll direction.
        /// </summary>
        /// <remarks>This method updates the value by a fixed amount determined by the scroll direction
        /// and resets the dragging state. It also triggers the scroll completion event.</remarks>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data, including the scroll delta.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Value += Math.Sign(e.Delta) * _thumbSize;
            _dragging = false;
            OnScrollCompleted();
        }

        /// <summary>
        /// Sets the current value based on the specified mouse location.
        /// </summary>
        /// <remarks>The method calculates the value as a percentage of the control's width or height,
        /// depending on its orientation. The calculated value is clamped between the minimum and maximum allowable
        /// values.</remarks>
        /// <param name="location">The location of the mouse, in client coordinates, used to determine the new value.</param>
        private void SetValueFromMouse(Point location)
        {
            float percent;

            if (_orientation == Orientation.Horizontal)
            {
                int left = 4;
                int right = Width - 4;
                percent = (float)(location.X - left) / (right - left);
            }
            else
            {
                int top = 4;
                int bottom = Height - 4;
                percent = (float)(bottom - location.Y) / (bottom - top);
            }

            percent = Math.Clamp(percent, 0f, 1f);
            Value = _minimum + (int)(percent * (_maximum - _minimum));
        }

        #endregion

        #region Events

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
        /// Handles the resize event of the control, ensuring that the control's dimensions do not fall below logical
        /// minimum values.
        /// </summary>
        /// <remarks>This method overrides the base <see cref="Control.OnResize"/> method to enforce
        /// minimum width and height constraints based on the control's orientation. For vertical orientation, the
        /// minimum width is 16 and the minimum height is 50. For horizontal orientation, the minimum width is 50 and
        /// the minimum height is 16. Controls is invalidated to ensure it updates</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        /// <summary>
        /// Occurs when the scroll position changes.
        /// </summary>
        /// <remarks>Subscribe to this event to perform actions in response to changes in the scroll
        /// position.</remarks>
        public event EventHandler? Scroll;

        /// <summary>
        /// Occurs when the value changes.
        /// </summary>
        /// <remarks>Subscribe to this event to be notified whenever the value is updated.  Ensure that
        /// event handlers are properly detached to prevent memory leaks.</remarks>
        public event EventHandler? ValueChanged;

        /// <summary>
        /// Occurs when a scroll operation has completed.
        /// </summary>
        /// <remarks>This event is triggered after the scrolling action is finished, allowing subscribers
        /// to perform actions based on the completion of the scroll. Ensure that any event handlers attached to this
        /// event are thread-safe.</remarks>
        public event EventHandler? ScrollCompleted;

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <remarks>This method is called whenever the value changes and is responsible for invoking the
        /// <see cref="ValueChanged"/> event. Derived classes can override this method to provide additional logic when
        /// the value changes.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="ScrollCompleted"/> event.
        /// </summary>
        /// <remarks>This method is called to signal that a scroll operation has completed.  Derived
        /// classes can override this method to provide additional logic  when the scroll operation finishes.</remarks>
        protected virtual void OnScrollCompleted() { ScrollCompleted?.Invoke(this, EventArgs.Empty); }

        #endregion

    }
}
