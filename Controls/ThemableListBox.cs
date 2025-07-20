using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;


namespace BazthalLib.Controls
{
    [DefaultProperty("Items")]
    [ToolboxItem(true)]
    public class ThemableListBox : ThemableControlBase
    {
        #region Fields
        private readonly string _version = "V1.2";
        private readonly ThemableScrollBarRenderer _scrollRenderer = new();
        //private ListBox.ObjectCollection _items;
        private NotifyingItemCollection _items;
        private readonly ListBox _hiddenListBox;

        private ThemeColors _themeColors = new();

        //private Color _selectedItemBackColor = SystemColors.Highlight;
        //private Color _selectedItemForeColor = SystemColors.HighlightText;

        private int _itemPadding = 5;
        private int _itemHeight = 17;
        private int _scrollValue = 0;
        private int _hoverIndex = -1; 
        private int _horizontalScrollValue = 0;
        private int _maxItemWidth = 0;
        private int _selectedIndex = -1;
        private bool _allowHoverHighlight = false;

        // private bool _showHorizontalScroll = false;
        private bool _alwaysShowScrollbars = false;
        private bool _showVerticalScroll = false;
        private bool _showHorizontalScroll = false;
        private bool _enableHorizontalScroll = true;
        // private bool _enableVerticalScroll = false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public bool SuppressDefaultItemDrawing { get; set; } = false;
        internal bool SuppressDefaultItemDrawing { get; set; } = false;

        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets the index of the currently hovered item.
        /// </summary>
        internal int HoverIndex { get => _hoverIndex; private set => _hoverIndex = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the border is enabled.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new bool EnableBorder { get => base.EnableBorder; set => base.EnableBorder = value; }

        /// <summary>
        /// Gets the unique identifier for the ThemableListBox control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string ControlID => $"ThemableListBox {_version}";


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < -1 || value >= Items.Count) return;
                _selectedIndex = value;
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TopIndex
        {
            get => _scrollValue;
            set
            {
                int clamped = Math.Max(0, Math.Min(value, ScrollMax));
                if (_scrollValue != clamped)
                {
                    _scrollValue = clamped;
                    Invalidate();
                }
            }
        }

/// <summary>
/// Gets or sets the foreground color of the selected item.
/// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Color SelectedItemForeColor
        {
            get => base.SelectedItemForeColor;
            set { base.SelectedItemForeColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the background color of the selected item.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Color SelectedItemBackColor
        {
            get => base.SelectedItemBackColor;
            set { base.SelectedItemBackColor = value; Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem
        {
            get => (_selectedIndex >= 0 && _selectedIndex < Items.Count) ? Items[_selectedIndex] : null;
            set
            {
                if (value == null)
                {
                    _selectedIndex = -1; // or another value that signifies "no selection"
                }
                else
                {
                    int index = Items.IndexOf(value);
                    if (index >= 0)
                    {
                        _selectedIndex = index;
                    }
                    else
                    {
                        BazthalLib.DebugUtils.Log("Selected Item", "ThemableListBox", "Item was not found in the list");
                    }
                }
            }
        }

        // public object? SelectedItem => (_selectedIndex >= 0 && _selectedIndex < Items.Count) ? Items[_selectedIndex] : null;

        /// <summary>
        /// Gets or sets a value indicating whether scrollbars are always displayed, even when not needed.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [DefaultValue(false)]
        [Description("Always show scrollbars even when not needed.")]
        public bool AlwaysShowScrollbars
        {
            get => _alwaysShowScrollbars;
            set { _alwaysShowScrollbars = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether horizontal scrolling is enabled.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [DefaultValue(true)]
        [Description("Enable or disable horizontal scrolling.")]
        public bool EnableHorizontalScroll
        {
            get => _enableHorizontalScroll;
            set { _enableHorizontalScroll = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the collection of items displayed in the list.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Data")]
        [Description("The items displayed in the list.")]

        
        public NotifyingItemCollection Items { get => _items; set { _items = value; Invalidate(); } }

        /// <summary>
        /// Gets or sets the height of each item in the control.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Item Height")]
        [DefaultValue(17)]
        public int ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = Math.Max(12, value); Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the padding applied to the left and right of items in the list.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Padding to the Left and Right of the items in the list")]
        [DefaultValue(5)]
        public int ItemPadding
        {
            get => _itemPadding;
            set { _itemPadding = value; Invalidate(); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the item under the cursor is highlighted when hovered over.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Allow for the item under the curser to be highlited")]
        [DefaultValue(false)]

        public bool AllowHoverHighlight
        {
            get => _allowHoverHighlight;
            set { _allowHoverHighlight = value; Invalidate(); }
        }
        
        

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        #endregion Properties

        #region Events

        public event EventHandler? SelectedIndexChanged;
        public event DrawItemEventHandler? DrawItem;
        public event EventHandler EnterPressed;
        public event EventHandler EscapePressed;

        protected virtual void OnSelectedIndexChanged(EventArgs e) => SelectedIndexChanged?.Invoke(this, e);

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableListBox"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableListBox"/> with double buffering enabled for smoother
        /// rendering, a default font of "Segoe UI" with size 9, and a default size of 120x94 pixels. It also configures the
        /// control to be selectable and sets up event handlers for mouse interactions. The text rendering mode is set to none,
        /// as text drawing will be handled manually. A hidden <see cref="ListBox"/> is used internally to manage
        /// items.</remarks>
        public ThemableListBox()
        {
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9);
            TextRenderMode = TextRenderMode.None; // We'll handle text drawing manually

            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            Size = new Size(120, 94);
            _hiddenListBox = new ListBox();
            //_items = new ListBox.ObjectCollection(_hiddenListBox);
            _items = new NotifyingItemCollection(new ListBox.ObjectCollection(_hiddenListBox), this);
            MouseWheel += HandleMouseWheel;
            MouseMove += HandleMouseMove;
            MouseLeave += (_, _) => { _hoverIndex = -1; Invalidate(); };
        }

        #endregion Events

        #region Event Handlers

        /// <summary>
        /// Handles vertical scrolling when the mouse wheel is used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseWheel(object? sender, MouseEventArgs e)
        {
            _scrollValue = Math.Max(0, Math.Min(_scrollValue - Math.Sign(e.Delta), ScrollMax));
            Invalidate();
        }

        /// <summary>
        /// Handles hover tracking as the mouse moves over items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMove(object? sender, MouseEventArgs e)
        {
             int index = (e.Y / _itemHeight) + _scrollValue;
                if (_hoverIndex != index && index < Items.Count)
                {
                    _hoverIndex = index;
                    Invalidate();
                } 
        }

        #endregion Event Handlers

        #region Overrides


        /// <summary>
        /// Handles visual updates when the control is resized.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// Identifies which key should be treated as input for custom processing.
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            return keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);
        }
        /// <summary>
        /// Handles keyboard navigation and selection logic
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Items.Count == 0)
                return;

            int newIndex = SelectedIndex == -1 ? 0 : SelectedIndex;
            int visibleCount = VisibleItems;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newIndex = Math.Max(0, newIndex - 1);
                    break;

                case Keys.Down:
                    newIndex = Math.Min(Items.Count - 1, newIndex + 1);
                    break;

                case Keys.PageUp:
                    newIndex = Math.Max(0, newIndex - visibleCount);
                    break;

                case Keys.PageDown:
                    newIndex = Math.Min(Items.Count - 1, newIndex + visibleCount);
                    break;

                case Keys.Home:
                    newIndex = 0;
                    break;

                case Keys.End:
                    newIndex = Items.Count - 1;
                    break;

                case Keys.Enter:
                    EnterPressed?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                    return;

                case Keys.Escape:
                    EscapePressed?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                    return;

                default:
                    return;
            }

            if (newIndex != SelectedIndex)
            {
                SelectedIndex = newIndex;

                // Scroll into view
                if (newIndex < _scrollValue)
                {
                    _scrollValue = newIndex;
                }
                else if (newIndex >= _scrollValue + visibleCount)
                {
                    _scrollValue = newIndex - visibleCount + 1;
                }

                _scrollValue = Math.Max(0, Math.Min(_scrollValue, ScrollMax));

                Invalidate();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles list item selection and scroll button interaction on mouse down
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (_showVerticalScroll)
            {
                var upBtn = new Rectangle(Width - 15, 0, 15, 15);
                var downBtn = new Rectangle(Width - 15, Height - 15, 15, 15);

                if (upBtn.Contains(e.Location))
                {
                    _scrollValue = Math.Max(0, _scrollValue - 1);
                    Invalidate();
                }
                else if (downBtn.Contains(e.Location))
                {
                    _scrollValue = Math.Min(ScrollMax, _scrollValue + 1);
                    Invalidate();
                }
            }

            if (_enableHorizontalScroll)
            {
                var leftBtn = new Rectangle(0, Height - 15, 15, 15);
                var rightBtn = new Rectangle(Width - 30, Height - 15, 15, 15);

                if (leftBtn.Contains(e.Location))
                {
                    _horizontalScrollValue = Math.Max(0, _horizontalScrollValue - 10);
                    Invalidate();
                }
                else if (rightBtn.Contains(e.Location))
                {
                    _horizontalScrollValue = Math.Min(_horizontalScrollValue + 10, GetMaxHorizontalScroll());
                    Invalidate();
                }
            }


            if (e.X < GetContentWidth())
            {
                int index = (e.Y / _itemHeight) + _scrollValue;
                if (index >= 0 && index < Items.Count)
                {
                    SelectedIndex = index;
                }
            }
        }
        /// <summary>
        /// Paints the control, including list items, scrollbars and focus rectagle
        /// </summary>
        /// <param name="e">Paint event arguments</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!SuppressDefaultItemDrawing || DrawItem != null)
                DrawItems(e.Graphics);

            if (_showVerticalScroll || _alwaysShowScrollbars)
                DrawVerticalScrollBar(e.Graphics);
            if ((_showHorizontalScroll || _alwaysShowScrollbars) && EnableHorizontalScroll)
                DrawHorizontalScrollBar(e.Graphics);

            base.OnPaint(e);

            if (Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(2, 2, Width - 4, Height - 4));

        }

        #endregion Overrides

        #region Drawing

        /// <summary>
        /// Draws the visible list items with support for custom rendering and selection/hover effects.
        /// </summary>
        /// <param name="g"></param>
        private void DrawItems(Graphics g)
        {
            _maxItemWidth = 0;

            for (int i = 0; i < VisibleItems; i++)
            {
                int index = i + _scrollValue;
                if (index >= Items.Count) break;

                bool isHovered = index == _hoverIndex && _allowHoverHighlight;
                bool isSelected = index == SelectedIndex;
                Color background = isSelected || isHovered ? _themeColors.SelectedItemBackColor : BackColor;
                Color foreground = isSelected || isHovered ? _themeColors.SelectedItemForeColor : ForeColor;

                string text = Items[index]?.ToString() ?? string.Empty;
                int itemWidth = TextRenderer.MeasureText(text, Font).Width;
                _maxItemWidth = Math.Max(_maxItemWidth, itemWidth);

                int contentWidth = GetContentWidth();
                Rectangle clipRect = new(0, i * _itemHeight, contentWidth, _itemHeight);
                Rectangle itemRect = new(_itemPadding - _horizontalScrollValue,i * _itemHeight,_maxItemWidth + 20,_itemHeight);

                g.SetClip(clipRect);

                var drawArgs = new DrawItemEventArgs(g, Font, itemRect, index,
                    isSelected ? DrawItemState.Selected : DrawItemState.Default, ForeColor, BackColor);

                if (DrawItem != null)
                {
                    DrawItem(this, drawArgs);
                }
                else
                {
                    using Brush bg = new SolidBrush(background);
                    using Brush fg = new SolidBrush(foreground);

                    g.FillRectangle(bg, clipRect);
                    g.DrawString(text, Font, fg, itemRect, new StringFormat
                    {
                        FormatFlags = StringFormatFlags.NoWrap,
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Near,
                        Trimming = StringTrimming.None
                    });
                }

                g.ResetClip();
                UpdateScrollVisibility();
            }
        }

        /// <summary>
        /// Draws the horizontal scrollbar using custom scrollbar renderer.
        /// </summary>
        /// <param name="g">Graphics context</param>
        private void DrawHorizontalScrollBar(Graphics g)
        {
            int verticalBarThickness = _showVerticalScroll ? 15 : 0;
            Rectangle scrollArea = new(0, Height - 15, Width - verticalBarThickness, 15);
            Rectangle leftBtn = new(scrollArea.Left, scrollArea.Top, 15, 15);
            Rectangle rightBtn = new(scrollArea.Right - 15, scrollArea.Top, 15, 15);
            Rectangle track = new(leftBtn.Right, scrollArea.Top, scrollArea.Width - 30, 15);

            int largeChange = Math.Max(1, Width - 15);

            Rectangle thumb = _scrollRenderer.GetThumbRectangle(
                Orientation.Horizontal, track, 0, _maxItemWidth, _horizontalScrollValue, largeChange);

            _scrollRenderer.DrawOuterBorder = true;
            _scrollRenderer.HoverArrows = false;
            _scrollRenderer.DrawScrollBar(g, scrollArea, Orientation.Horizontal, leftBtn, rightBtn, track, thumb);
        }

        /// <summary>
        /// Draws the vertical scrollbar using custom scrollbar renderer.
        /// </summary>
        /// <param name="g">Graphics context</param>
        private void DrawVerticalScrollBar(Graphics g)
        {
            int largeChange = Math.Max(1, VisibleItems);
            Rectangle scrollbarRect = new(Width - 15, 0, 15, Height);
            Rectangle upBtn = new(scrollbarRect.Left, scrollbarRect.Top, 15, 15);
            Rectangle downBtn = new(scrollbarRect.Left, scrollbarRect.Bottom - 15, 15, 15);
            Rectangle track = new(scrollbarRect.Left, upBtn.Bottom, 15, Height - 30);

            Rectangle thumb = _scrollRenderer.GetThumbRectangle(
                Orientation.Vertical, track, 0, _items.Count, _scrollValue, largeChange);

            _scrollRenderer.DrawOuterBorder = true;
            _scrollRenderer.HoverArrows = false;
            _scrollRenderer.DrawScrollBar(g, scrollbarRect, Orientation.Vertical, upBtn, downBtn, track, thumb);
        }

        #endregion Drawing

        #region Helpers
        /// <summary>
        /// Determines the index of the item at the specified point within the control.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to evaluate, typically representing a location within the control.</param>
        /// <returns>The zero-based index of the item at the specified point if the point is within the bounds of an item;
        /// otherwise, -1.</returns>
        public int IndexFromPoint(Point point)
        {
            int index = (point.Y / ItemHeight) + TopIndex;
            return (index >= 0 && index < Items.Count) ? index : -1;
        }
        public void EnsureVisible(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            if (index < _scrollValue)
                TopIndex = index;
            else if (index >= _scrollValue + VisibleItems)
                TopIndex = index - VisibleItems + 1;
        }

        /// <summary>
        /// Updates the visibility of the scroll bars based on the current item dimensions and container size.
        /// </summary>
        /// <remarks>Determines whether vertical and horizontal scroll bars should be displayed by
        /// comparing the total item height and maximum item width against the container's dimensions.</remarks>
        private void UpdateScrollVisibility()
        {
            _showVerticalScroll = Items.Count * _itemHeight > Height;
            _showHorizontalScroll = _maxItemWidth > (Width - (_showVerticalScroll ? 30 : 0));
        }
        /// <summary>
        /// Calculates the width of the content area, adjusting for the presence of a vertical scrollbar.
        /// </summary>
        /// <returns>The width of the content area in pixels. If a vertical scrollbar is shown, the width is reduced by 15
        /// pixels.</returns>
        private int GetContentWidth() => Width - (_showVerticalScroll ? 15 : 0);

        /// <summary>
        /// Calculates the maximum horizontal scroll value based on the current width and item size.
        /// </summary>
        /// <returns>The maximum number of pixels that can be scrolled horizontally. Returns 0 if no scrolling is needed.</returns>
        private int GetMaxHorizontalScroll()
        {
            int visibleWidth = Width - (_showVerticalScroll ? 15 : 0);
            return Math.Max(0, _maxItemWidth - visibleWidth);
        }
        
        private int VisibleItems => Height / _itemHeight;

        private int ScrollMax => Math.Max(0, Items.Count - VisibleItems);
        /// <summary>
        /// Applies the specified theme colors to the scroll renderer.
        /// </summary>
        /// <remarks>This method updates the background, border, accent, and foreground colors of the
        /// scroll renderer based on the provided theme colors. If <see cref="UseThemeColors"/> is <see
        /// langword="false"/> or <paramref name="colors"/> is <see langword="null"/>, the method returns without making
        /// changes.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public override void ApplyTheme(ThemeColors colors)
        {
            if (!UseThemeColors || colors == null)
                return;

            base.ApplyTheme(colors);

            _themeColors = colors;
            _scrollRenderer.BackColor = colors.BackColor;
            _scrollRenderer.BorderColor = colors.BorderColor;
            _scrollRenderer.AccentColor = colors.AccentColor;
            _scrollRenderer.ForeColor = colors.ForeColor;

            Invalidate();

        }

        #endregion Helpers


        #region Wrappers

/// <summary>
/// Retrieves the text representation of the specified item.
/// </summary>
/// <param name="item">The item for which to retrieve the text representation. This can be any object.</param>
/// <returns>The text representation of the specified item, as determined by the underlying list box.</returns>
        public string GetItemText(object item)
        {
            return _hiddenListBox.GetItemText(item);
        }

        #endregion
    }
}
