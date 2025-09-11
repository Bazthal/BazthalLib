using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BazthalLib.UI;


namespace BazthalLib.Controls
{
    [DefaultProperty("Items")]
    [ToolboxItem(true)]
    public class ThemableListBox : ThemableControlBase
    {
        #region Fields
        private readonly string _version = "V1.3";
        private readonly ThemableScrollBarRenderer _scrollRenderer = new();
        private NotifyingItemCollection _items;
        private readonly ListBox _hiddenListBox;

        private ThemeColors _themeColors = new();
        private int _itemPadding = 5;
        private int _itemHeight = 17;
        private int _scrollValue = 0;
        private int _hoverIndex = -1; 
        private int _horizontalScrollValue = 0;
        private int _maxItemWidth = 0;
      //  private int _selectedIndex = -1;
        private bool _allowHoverHighlight = false;
        private int _topPadding = 2;
        private int _bottomPadding = 2;

        private readonly HashSet<int> _selectedIndices = new();
        private int _lastSelectedIndex = -1;

        private bool _enableMultiSelect = false;

        private readonly ThemableScrollBar _vScrollBar;
        private readonly ThemableScrollBar _hScrollBar;

        private bool _alwaysShowScrollbars = false;

        private bool _enableHorizontalScroll = true;
        private bool _scrollHoverArrows = true;
        // private bool _enableVerticalScroll = false;

        private string _searchBuffer = string.Empty;
        private DateTime _lastKeyPressTime;
        private int _lastSearchIndex = -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool SuppressDefaultItemDrawing { get; set; } = false;

        /// <summary>
        /// Specifies the modes for matching search terms in a string.
        /// </summary>
        /// <remarks>This enumeration defines the strategies for matching search terms, such as matching
        /// only the prefix,  matching the prefix of individual words, or matching any substring within the target
        /// string.</remarks>
        public enum SearchMatchMode
        {
            PrefixOnly,
            WordPrefix,
            Substring     
        }


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
        public IReadOnlyCollection<int> SelectedIndices => _selectedIndices;

        [Browsable(false)]
        public IEnumerable<object> SelectedItems => _selectedIndices.Select(i => Items[i]);

        /// <summary>
        /// Gets or sets the zero-based index of the currently selected item in the list.
        /// </summary>
        /// <remarks>Setting this property to a value outside the valid range (less than 0 or greater than
        /// or equal to the number of items)  will result in no item being selected. When the selection changes, the
        /// <see cref="OnSelectedIndexChanged"/> event is raised.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndices.FirstOrDefault();
            set
            {
                _selectedIndices.Clear();
                if (value >= 0 && value < Items.Count)
                    _selectedIndices.Add(value);

                _lastSelectedIndex = value;
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the index of the first visible item in the scrollable area.
        /// </summary>
        /// <remarks>Setting this property adjusts the scroll position to ensure the specified item is at
        /// the top of the visible area. If the value exceeds the maximum scrollable index, it is automatically clamped.
        /// Changes to this property may trigger a redraw of the control.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TopIndex
        {
            get => _scrollValue;
            set
            {
                int clamped = Math.Max(0, Math.Min(value, ScrollMax));
                _horizontalScrollValue = Math.Max(0, Math.Min(_horizontalScrollValue, GetMaxHorizontalScroll()));
                if (_scrollValue != clamped)
                {
                    _scrollValue = clamped;
                    if (_vScrollBar.Visible)
                        _vScrollBar.Value = _scrollValue;
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

        /// <summary>
        /// Gets or sets a value indicating whether type-to-search always selects the first match  instead of cycling
        /// through matches.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("If true, type-to-search always selects the first match instead of cycling.")]
        [DefaultValue(false)]
        public bool AlwaysSelectFirstMatch { get; set; } = false;

        /// <summary>
        /// Gets or sets the extra padding, in pixels, at the top before the first item text is drawn.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Extra padding at the very top before the first item text is drawn.")]
        [DefaultValue(2)]
        public int TopPadding
        {
            get => _topPadding;
            set { _topPadding = Math.Max(0, value); Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the extra padding at the bottom after the last item's text.
        /// </summary>
        /// <remarks>Setting this property to a value less than 0 will automatically adjust it to 0.
        /// Changing this property will cause the control to be redrawn.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Extra padding at the very bottom after the last item text.")]
        [DefaultValue(2)]
        public int BottomPadding
        {
            get => _bottomPadding;
            set { _bottomPadding = Math.Max(0, value); Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the first selected item in the list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="EnableMultiSelect"/> is <c>false</c>, this property behaves like the standard
        /// WinForms <c>ListBox.SelectedItem</c>, returning the single selected item or <c>null</c> if no
        /// item is selected.
        /// </para>
        /// <para>
        /// When <see cref="EnableMultiSelect"/> is <c>true</c>, this property returns the first selected
        /// item based on the lowest selected index, or <c>null</c> if no items are selected.
        /// To retrieve all selected items in multi-select mode, use <see cref="SelectedItems"/>.
        /// </para>
        /// </remarks>
        /// <value>
        /// The first selected item in the list, or <c>null</c> if no selection exists.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem
        {
            get
            {
                if (EnableMultiSelect)
                {
                    if (_selectedIndices.Count == 0) return null;
                    int firstIndex = _selectedIndices.Min();
                    return (firstIndex >= 0 && firstIndex < Items.Count) ? Items[firstIndex] : null;
                }
                else
                {
                    return (_selectedIndices.Count == 1) ? Items[_selectedIndices.First()] : null;
                }
            }
            set
            {
                _selectedIndices.Clear();
                if (value != null)
                {
                    int index = Items.IndexOf(value);
                    if (index >= 0)
                        _selectedIndices.Add(index);
                }
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether multi-select behavior is enabled, allowing the use of  Ctrl or Shift
        /// modifiers to select multiple items.
        /// </summary>
        /// <remarks>When multi-select is enabled, users can hold the Ctrl or Shift key to select multiple
        /// items. Changing this property clears the current selection.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Enable multi-select behavior with Ctrl/Shift modifiers.")]
        [DefaultValue(false)]
        public bool EnableMultiSelect
        {
            get => _enableMultiSelect;
            set
            {
                if (_enableMultiSelect != value)
                {
                    _enableMultiSelect = value;
                    ClearSelection();
                }
            }
        }

        /// <summary>
        /// Gets or sets the mode used to match typed text against items in the list.
        /// </summary>
        /// <remarks>This property controls the behavior of text matching, such as whether the match is
        /// based  on the prefix, contains, or other criteria defined by the <see cref="SearchMatchMode"/>
        /// enumeration.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("How typed text is matched against items in the list.")]
        [DefaultValue(SearchMatchMode.PrefixOnly)]
        public SearchMatchMode MatchMode { get; set; } = SearchMatchMode.PrefixOnly;

        /// <summary>
        /// Gets or sets the time, in milliseconds, after which the type-to-search buffer resets.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Milliseconds after which the type-to-search buffer resets.")]
        [DefaultValue(1000)]
        public int SearchTimeout { get; set; } = 1000;


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentSearchBuffer => _searchBuffer;

        /// <summary>
        /// Gets or sets a value indicating whether the space character is included in the type-to-search buffer.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Include the space character in the type-to-search buffer.")]
        [DefaultValue(true)]
        public bool IncludeSpaceInSearchBuffer { get; set; } = true;

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


        /// <summary>
        /// Gets or sets a value indicating whether scrollbar arrows are visible only on hover or always visible.
        /// </summary>
        /// <remarks>Changing this property updates the visibility behavior of the arrows for both
        /// vertical and horizontal scrollbars.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Show scrollbar arrows only on hover, or always visible.")]
        [DefaultValue(true)]
        public bool ScrollHoverArrows
        {
            get => _scrollHoverArrows;
            set
            {
                _scrollHoverArrows = value;
                if (_vScrollBar != null) _vScrollBar.HoverArrows = value;
                if (_hScrollBar != null) _hScrollBar.HoverArrows = value;
                Invalidate();
            }
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

        public event EventHandler<string>? SearchBufferChanged;

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
            TextRenderMode = TextRenderMode.None;

            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            Size = new Size(120, 94);

            _hiddenListBox = new ListBox();
            _items = new NotifyingItemCollection(new ListBox.ObjectCollection(_hiddenListBox), this);

            // Vertical scrollbar
            _vScrollBar = new ThemableScrollBar
            {
                Orientation = Orientation.Vertical,
                Dock = DockStyle.Right,
                Visible = false,
                HoverArrows = _scrollHoverArrows
            };
            _vScrollBar.ValueChanged += (s, e) =>
            {
                _scrollValue = _vScrollBar.Value;
                Invalidate();
            };
            Controls.Add(_vScrollBar);

            // Horizontal scrollbar
            _hScrollBar = new ThemableScrollBar
            {
                Orientation = Orientation.Horizontal,
                Dock = DockStyle.Bottom,
                Visible = false,
                HoverArrows = _scrollHoverArrows
            };
            _hScrollBar.ValueChanged += (s, e) =>
            {
                _horizontalScrollValue = _hScrollBar.Value;
                Invalidate();
            };
            Controls.Add(_hScrollBar);

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
            int index = ((e.Y - _topPadding) / _itemHeight) + _scrollValue;
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
        /// Handles key press events to provide navigation, selection, and other interactions within the control.
        /// </summary>
        /// <remarks>This method supports various key-based interactions: <list type="bullet"> <item>
        /// <description>Arrow keys (<see cref="Keys.Up"/> and <see cref="Keys.Down"/>) navigate through items one at a
        /// time.</description> </item> <item> <description>Page navigation keys (<see cref="Keys.PageUp"/> and <see
        /// cref="Keys.PageDown"/>) navigate by the number of visible items.</description> </item> <item>
        /// <description><see cref="Keys.Home"/> and <see cref="Keys.End"/> jump to the first and last items,
        /// respectively.</description> </item> <item> <description><see cref="Keys.Enter"/> and <see
        /// cref="Keys.Escape"/> trigger the <see cref="EnterPressed"/> and <see cref="EscapePressed"/>
        /// events.</description> </item> <item> <description><see cref="Keys.Back"/> and <see cref="Keys.Delete"/>
        /// clear or modify the search buffer.</description> </item> <item> <description>If <see
        /// cref="EnableMultiSelect"/> is <see langword="true"/>, modifier keys like <see cref="Keys.Control"/> and <see
        /// cref="Keys.Shift"/> allow multi-selection.</description> </item> </list> The method also adjusts the scroll
        /// position to ensure the selected item remains visible and raises the <see cref="OnSelectedIndexChanged"/>
        /// event when the selection changes.</remarks>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data, including the key pressed and modifier keys.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (EnableMultiSelect && e.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
                e.Handled = true;
                return;
            }

            {
                base.OnKeyDown(e);

                if (Items.Count == 0)
                    return;

                int newIndex = _lastSelectedIndex == -1 ? 0 : _lastSelectedIndex;
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

                    case Keys.Back:               
                        HandleBackspace();       
                        e.Handled = true;
                        return;

                    case Keys.Delete:          
                        ClearSearchBuffer();
                        e.Handled = true;
                        return;
                    default:
                        return;
                }

                if (EnableMultiSelect)
                {
                    if (ModifierKeys.HasFlag(Keys.Control | Keys.Shift) && _lastSelectedIndex != -1)
                    {
                        int start = Math.Min(_lastSelectedIndex, newIndex);
                        int end = Math.Max(_lastSelectedIndex, newIndex);
                        for (int i = start; i <= end; i++)
                            _selectedIndices.Add(i);
                    }
                    else if (ModifierKeys.HasFlag(Keys.Shift) && _lastSelectedIndex != -1)
                    {
                        _selectedIndices.Clear();
                        int start = Math.Min(_lastSelectedIndex, newIndex);
                        int end = Math.Max(_lastSelectedIndex, newIndex);
                        for (int i = start; i <= end; i++)
                            _selectedIndices.Add(i);
                    }
                    else if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        _selectedIndices.Add(newIndex);
                    }
                    else
                    {
                        _selectedIndices.Clear();
                        _selectedIndices.Add(newIndex);
                    }
                }
                else
                {
                    _selectedIndices.Clear();
                    _selectedIndices.Add(newIndex);
                }

                _lastSelectedIndex = newIndex;

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
                OnSelectedIndexChanged(EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="KeyPress"/> event for the control.
        /// </summary>
        /// <remarks>This method processes key press events and performs additional actions  specific to
        /// the control. It ensures that the base class implementation  is invoked before handling the event.</remarks>
        /// <param name="e">A <see cref="KeyPressEventArgs"/> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            HandleTypeToSearch(e);
        }

        /// <summary>
        /// Handles the mouse down event for the control, enabling interaction with scroll buttons and item selection.
        /// </summary>
        /// <remarks>This method provides functionality for vertical and horizontal scrolling when the
        /// corresponding scroll buttons are clicked. It also supports item selection, including multi-selection with
        /// modifier keys such as <see cref="Keys.Control"/> and <see cref="Keys.Shift"/>.  - Clicking the vertical
        /// scroll buttons adjusts the vertical scroll position. - Clicking the horizontal scroll buttons adjusts the
        /// horizontal scroll position, if enabled. - Clicking within the content area selects an item based on the
        /// mouse position. Multi-selection is supported when enabled.  The method ensures the control gains focus when
        /// clicked and invalidates the control to trigger a redraw after state changes.</remarks>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data, including the mouse location and button state.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (_vScrollBar != null && _vScrollBar.Visible)
            {
                var upBtn = new Rectangle(Width - 15, 0, 15, 15);
                var downBtn = new Rectangle(Width - 15, Height - 15, 15, 15);

                if (upBtn.Contains(e.Location))
                {
                    _scrollValue = Math.Max(0, _scrollValue - 1);
                    Invalidate();
                    return;
                }
                else if (downBtn.Contains(e.Location))
                {
                    _scrollValue = Math.Min(ScrollMax, _scrollValue + 1);
                    Invalidate();
                    return;
                }
            }

            if (_hScrollBar != null && _hScrollBar.Visible)
            {
                var leftBtn = new Rectangle(0, Height - 15, 15, 15);
                var rightBtn = new Rectangle(Width - 30, Height - 15, 15, 15);

                if (leftBtn.Contains(e.Location))
                {
                    _horizontalScrollValue = Math.Max(0, _horizontalScrollValue - 10);
                    Invalidate();
                    return;
                }
                else if (rightBtn.Contains(e.Location))
                {
                    _horizontalScrollValue = Math.Min(_horizontalScrollValue + 10, GetMaxHorizontalScroll());
                    Invalidate();
                    return;
                }
            }

            if (e.X < GetContentWidth())
            {
                int index = ((e.Y - _topPadding) / _itemHeight) + _scrollValue;
                if (index < 0 || index >= Items.Count) return;

                if (EnableMultiSelect)
                {
                    if (ModifierKeys.HasFlag(Keys.Control | Keys.Shift) && _lastSelectedIndex != -1)
                    {
                        int start = Math.Min(_lastSelectedIndex, index);
                        int end = Math.Max(_lastSelectedIndex, index);
                        for (int i = start; i <= end; i++)
                            _selectedIndices.Add(i);
                    }
                    else if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        if (_selectedIndices.Contains(index))
                            _selectedIndices.Remove(index);
                        else
                            _selectedIndices.Add(index);
                    }
                    else if (ModifierKeys.HasFlag(Keys.Shift) && _lastSelectedIndex != -1)
                    {
                        _selectedIndices.Clear();
                        int start = Math.Min(_lastSelectedIndex, index);
                        int end = Math.Max(_lastSelectedIndex, index);
                        for (int i = start; i <= end; i++)
                            _selectedIndices.Add(i);
                    }
                    else
                    {
                        _selectedIndices.Clear();
                        _selectedIndices.Add(index);
                    }
                }
                else
                {
                    _selectedIndices.Clear();
                    _selectedIndices.Add(index);
                }

                _lastSelectedIndex = index;
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Paints the control, including list items, scrollbars and focus rectagle
        /// </summary>
        /// <param name="e">Paint event arguments</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!SuppressDefaultItemDrawing || DrawItem != null)
                DrawItems(e.Graphics);

            // Draw the corner padding so bars don't collide in the corner
            var corner = ScrollCornerRect;
            if (!corner.IsEmpty)
            {
                using var bg = new SolidBrush(_themeColors.BackColor);
                e.Graphics.FillRectangle(bg, corner);

                using var border = new Pen(_themeColors.BorderColor);
                e.Graphics.DrawRectangle(border, new Rectangle(corner.X, corner.Y, corner.Width - 1, corner.Height - 1));
            }

            DrawBorder(e.Graphics);

            int bottomPadding = (_hScrollBar != null && _hScrollBar.Visible) ? _hScrollBar.Height + 2 : 4;
            int rightPadding = (_vScrollBar != null && _vScrollBar.Visible) ? _vScrollBar.Width + 2  : 4;
            if (Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(2, 2, Width - rightPadding, Height - bottomPadding));
        }

        #endregion Overrides

        #region Drawing

        /// <summary>
        /// Renders the visible items in the control onto the specified <see cref="Graphics"/> surface.
        /// </summary>
        /// <remarks>This method iterates through the visible items in the control, applying appropriate
        /// styles for  selected and hovered items, and renders them using the provided <see cref="Graphics"/> context. 
        /// If a custom <see cref="DrawItem"/> event handler is defined, it is invoked for each item;  otherwise, the
        /// default rendering logic is used.  The method also calculates the maximum item width for layout purposes and
        /// updates the visibility  of the scrollbars based on the content size.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the items.</param>
        private void DrawItems(Graphics g)
        {
            int contentWidth = GetContentWidth();
            int contentHeight = GetContentHeight();

            for (int i = 0; i < VisibleItems; i++)
            {
                int index = i + _scrollValue;
                if (index >= Items.Count) break;

                int y = _topPadding + (i * _itemHeight);

                if (y + _itemHeight > contentHeight - _bottomPadding)
                    break;

                bool isHovered = index == _hoverIndex && _allowHoverHighlight;
                bool isSelected = _selectedIndices.Contains(index);
                Color background = isSelected || isHovered ? _themeColors.SelectedItemBackColor : BackColor;
                Color foreground = isSelected || isHovered ? _themeColors.SelectedItemForeColor : ForeColor;

                string text = Items[index]?.ToString() ?? string.Empty;
                int itemWidth = TextRenderer.MeasureText(text, Font).Width;
                _maxItemWidth = Math.Max(_maxItemWidth, itemWidth);

                Rectangle clipRect = new(0, y, contentWidth, _itemHeight);
                Rectangle itemRect = new(_itemPadding - _horizontalScrollValue, y,
                                          _maxItemWidth + 20, _itemHeight);

                g.SetClip(clipRect);

                var drawArgs = new DrawItemEventArgs(g, Font, itemRect, index,
                    isSelected ? DrawItemState.Selected : DrawItemState.Default,
                    ForeColor, BackColor);

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
            }

            UpdateScrollVisibility();
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

        /// <summary>
        /// Ensures that the item at the specified index is visible within the scrollable area.
        /// </summary>
        /// <remarks>If the specified index is already visible, no action is taken. If the index is less
        /// than the current scroll position, the scrollable area is adjusted to bring the item to the top. If the index
        /// is beyond the currently visible range, the scrollable area is adjusted to bring the item into view at the
        /// bottom.</remarks>
        /// <param name="index">The zero-based index of the item to make visible. Must be within the range of valid indices for the <see
        /// cref="Items"/> collection.</param>
        public void EnsureVisible(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            if (index < _scrollValue)
                TopIndex = index;
            else if (index >= _scrollValue + VisibleItems)
                TopIndex = index - VisibleItems + 1;
        }

        /// <summary>
        /// Updates the visibility and configuration of the vertical and horizontal scrollbars  based on the current
        /// content size, container dimensions, and scrolling settings.
        /// </summary>
        /// <remarks>This method evaluates whether vertical and horizontal scrollbars are needed by 
        /// considering the number of items, their dimensions, and the available space in the container.  It respects
        /// the <c>_enableHorizontalScroll</c> flag to determine if horizontal scrolling is allowed  and applies the
        /// <c>_alwaysShowScrollbars</c> setting to force scrollbar visibility when necessary.  If horizontal scrolling
        /// is disabled, the horizontal scroll offset is reset to zero.  The method also synchronizes the scrollbar
        /// ranges, large changes, small changes, and current values  to ensure they reflect the current content and
        /// container state.</remarks>
        private void UpdateScrollVisibility()
        {
            bool verticalNeeded = Items.Count * _itemHeight > Height;
            bool horizontalAllowed = _enableHorizontalScroll;

            int widthForContent = Width - (verticalNeeded ? (_vScrollBar?.Width ?? 15) : 0);
            bool horizontalNeeded = horizontalAllowed && (_maxItemWidth > widthForContent);

            int heightForContent = Height - (horizontalNeeded ? (_hScrollBar?.Height ?? 15) : 0);
            verticalNeeded = Items.Count * _itemHeight > heightForContent;

            bool vVisible = _alwaysShowScrollbars || verticalNeeded;
            bool hVisible = horizontalAllowed && (_alwaysShowScrollbars || horizontalNeeded);

            if (_vScrollBar != null) _vScrollBar.Visible = vVisible;
            if (_hScrollBar != null) _hScrollBar.Visible = hVisible;

            if (!horizontalAllowed)
                _horizontalScrollValue = 0;

            if (_vScrollBar != null && _vScrollBar.Visible)
            {
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = Math.Max(Items.Count, 1);
                _vScrollBar.LargeChange = Math.Max(1, VisibleItems);
                _vScrollBar.SmallChange = 1;
                _vScrollBar.Value = Math.Max(0, Math.Min(_scrollValue, ScrollMax));
            }

            if (_hScrollBar != null && _hScrollBar.Visible)
            {
                _hScrollBar.Minimum = 0;
                _hScrollBar.Maximum = Math.Max(_maxItemWidth, 1);
                _hScrollBar.LargeChange = Math.Max(1, GetContentWidth());
                _hScrollBar.SmallChange = 10;
                _hScrollBar.Value = Math.Max(0, Math.Min(_horizontalScrollValue, GetMaxHorizontalScroll()));
            }
        }

        /// <summary>
        /// Calculates the available width for content, accounting for the visibility of a vertical scrollbar.
        /// </summary>
        /// <returns>The width available for content, in pixels. If a vertical scrollbar is visible, its width is subtracted from
        /// the total width.</returns>
        private int GetContentWidth()
        {
            return Width - (_vScrollBar != null && _vScrollBar.Visible ? _vScrollBar.Width : 0);
        }

        /// <summary>
        /// Calculates the height of the content area, accounting for the visibility of the horizontal scrollbar.
        /// </summary>
        /// <returns>The height of the content area in pixels. If a horizontal scrollbar is visible, its height is subtracted
        /// from the total height.</returns>
        private int GetContentHeight()
        {
            return Height - (_hScrollBar != null && _hScrollBar.Visible ? _hScrollBar.Height : 0);
        }

        /// <summary>
        /// Gets the rectangle representing the scroll corner area where the vertical and horizontal scrollbars
        /// intersect.
        /// </summary>
        private Rectangle ScrollCornerRect
        {
            get
            {
                if (_vScrollBar != null && _vScrollBar.Visible &&
                    _hScrollBar != null && _hScrollBar.Visible)
                {
                    return new Rectangle(
                        Width - _vScrollBar.Width,
                        Height - _hScrollBar.Height,
                        _vScrollBar.Width,
                        _hScrollBar.Height
                    );
                }
                return Rectangle.Empty;
            }
        }

        /// <summary>
        /// Calculates the maximum horizontal scroll value based on the current width and item size.
        /// </summary>
        /// <returns>The maximum number of pixels that can be scrolled horizontally. Returns 0 if no scrolling is needed.</returns>
        private int GetMaxHorizontalScroll()
        {
            int visibleWidth = GetContentWidth();
            return Math.Max(0, _maxItemWidth - visibleWidth);
        }

        /// <summary>
        /// Gets the number of items that are visible within the current content area.
        /// </summary>
        /// <remarks>The value is calculated based on the content height, padding, and item
        /// height.</remarks>
        private int VisibleItems => Math.Max(1, (GetContentHeight() - (_topPadding + _bottomPadding)) / _itemHeight);

        /// <summary>
        /// Gets the maximum scroll offset, representing the highest valid position  for scrolling based on the total
        /// number of items and the number of visible items.
        /// </summary>
        private int ScrollMax => Math.Max(0, Items.Count - VisibleItems);

        /// <summary>
        /// Clears all selected items in the current selection.
        /// </summary>
        /// <remarks>This method removes all indices from the selection and triggers a redraw of the
        /// control  to reflect the updated state. After calling this method, no items will be selected.</remarks>
        public void ClearSelection()
        {
            _selectedIndices.Clear();
            Invalidate();
        }

        /// <summary>
        /// Selects all items in the collection if multi-select is enabled.
        /// </summary>
        /// <remarks>This method clears the current selection and selects all items in the collection.  It
        /// has no effect if multi-select is disabled.</remarks>
        public void SelectAll()
        {
            if (!EnableMultiSelect) return;

            _selectedIndices.Clear();
            for (int i = 0; i < Items.Count; i++)
                _selectedIndices.Add(i);
            Invalidate();
        }

        /// <summary>
        /// Handles the key press event to perform a search based on the typed character(s).
        /// </summary>
        /// <remarks>This method processes the key press to build a search buffer and perform a search
        /// operation.  If the same key is pressed repeatedly, it cycles through matching results. If a new key is 
        /// pressed or the buffer has expired, it resets the search buffer and starts a new search.  The method ignores
        /// control characters and whitespace. The search behavior is influenced by  the time elapsed since the last key
        /// press, determined by <c>SearchTimeoutMs</c>.</remarks>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> containing the key press data.</param>
        private void HandleTypeToSearch(KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) &&
                !(IncludeSpaceInSearchBuffer && e.KeyChar == ' '))
                return;

            if (e.KeyChar == ' ' && !IncludeSpaceInSearchBuffer)
                return;

            string keyChar = e.KeyChar.ToString();
            bool bufferExpired = (DateTime.Now - _lastKeyPressTime).TotalMilliseconds > SearchTimeout;

            if (bufferExpired)
            {
                _searchBuffer = string.Empty;
                _lastSearchIndex = -1;
            }

            if (_searchBuffer.Length == 1 &&
                _searchBuffer.Equals(keyChar, StringComparison.CurrentCultureIgnoreCase))
            {
                SearchAndSelect(keyChar, cycle: true);
            }
            else
            {
                _searchBuffer += keyChar;
                SearchAndSelect(_searchBuffer, cycle: false);
            }

            _lastKeyPressTime = DateTime.Now;
            SearchBufferChanged?.Invoke(this, _searchBuffer);
        }

        /// <summary>
        /// Searches for an item in the collection that matches the specified search string and selects it.
        /// </summary>
        /// <remarks>The search behavior is determined by the <see cref="MatchMode"/> property, which
        /// specifies how the search string is matched: <list type="bullet"> <item><description><see
        /// cref="SearchMatchMode.PrefixOnly"/>: Matches items that start with the search string.</description></item>
        /// <item><description><see cref="SearchMatchMode.WordPrefix"/>: Matches items where any word starts with the
        /// search string.</description></item> <item><description><see cref="SearchMatchMode.Substring"/>: Matches
        /// items that contain the search string anywhere.</description></item> </list> If <see
        /// cref="AlwaysSelectFirstMatch"/> is <see langword="true"/>, the search always starts from the beginning of
        /// the collection. Otherwise, the search starts from the item after the last matched item, if
        /// available.</remarks>
        /// <param name="search">The search string to match against the items in the collection. The comparison is case-insensitive.</param>
        /// <param name="cycle">A value indicating whether the search should continue from the beginning of the collection after reaching
        /// the end. If <see langword="true"/>, the search wraps around to the start of the collection; otherwise, it
        /// stops at the end.</param>
        private void SearchAndSelect(string search, bool cycle)
        {
            if (Items.Count == 0) return;

            int startIndex;

            if (AlwaysSelectFirstMatch)
            {
                startIndex = 0;
            }
            else if (cycle && _lastSearchIndex != -1)
            {
                startIndex = (_lastSearchIndex + 1) % Items.Count;
            }
            else
            {
                startIndex = 0;
            }

            for (int offset = 0; offset < Items.Count; offset++)
            {
                int i = (startIndex + offset) % Items.Count;
                string itemText = GetItemText(Items[i]) ?? string.Empty;

                bool isMatch = MatchMode switch
                {
                    SearchMatchMode.PrefixOnly =>
                        itemText.StartsWith(search, StringComparison.CurrentCultureIgnoreCase),

                    SearchMatchMode.WordPrefix =>
                        itemText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Any(word => word.StartsWith(search, StringComparison.CurrentCultureIgnoreCase)),

                    SearchMatchMode.Substring =>
                        itemText.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0,

                    _ => false
                };

                if (isMatch)
                {
                    SelectedIndex = i;
                    EnsureVisible(i);
                    _lastSearchIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the backspace key press by removing the last character from the search buffer.
        /// </summary>
        /// <remarks>If the search buffer becomes empty after removing the last character, the selection
        /// is cleared  and the display is redrawn. Otherwise, the method attempts to search and select based on the 
        /// updated search buffer. The method also updates the timestamp of the last key press.</remarks>
        private void HandleBackspace()
        {
            if (string.IsNullOrEmpty(_searchBuffer))
                return;

            _searchBuffer = _searchBuffer.Substring(0, _searchBuffer.Length - 1);
            _lastSearchIndex = -1;

            if (_searchBuffer.Length > 0)
            {
                SearchAndSelect(_searchBuffer, cycle: false);
            }
            else
            {
                Invalidate(); 
            }
            _lastKeyPressTime = DateTime.Now;
            SearchBufferChanged?.Invoke(this, _searchBuffer);
        }

        /// <summary>
        /// Clears the current search buffer and resets related search state.
        /// </summary>
        /// <remarks>This method resets the search buffer to an empty string, clears the last search
        /// index,  and updates the last key press timestamp. It also triggers a redraw of the control  to reflect the
        /// cleared state.</remarks>
        private void ClearSearchBuffer()
        {
            if (string.IsNullOrEmpty(_searchBuffer))
                return;

            _searchBuffer = string.Empty;
            _lastSearchIndex = -1;
            Invalidate();

            _lastKeyPressTime = DateTime.Now;
            SearchBufferChanged?.Invoke(this, _searchBuffer);
        }

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
