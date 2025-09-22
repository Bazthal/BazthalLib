using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BazthalLib.UI;
using BazthalLib.Events;


namespace BazthalLib.Controls
{
    [DefaultProperty("Items")]
    [ToolboxItem(true)]
    public class ThemableListBox : ThemableControlBase
    {
        #region Fields
        private readonly string _version = "V1.4";
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

        private int _dropIndex = -1;
        private List<int> _dragIndices = new();

        
        private const string ReorderFormat = "BazthalLib.Controls.ThemableListBox/Reorder";
        private Point _dragStartPoint;
        private bool _dragPending;
        private Bitmap? _dragGhost;
        private Point _ghostOffset;
        private float _dragGhostOpacity = 0.5f;
        private Point _ghostPosition;


        private DragGhostSizeMode _dragGhostSizeMode = DragGhostSizeMode.FillControl;
        private int _dragGhostFixedWidth = 200;


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

        /// <summary>
        /// Specifies the sizing behavior of a drag ghost element during drag-and-drop operations.
        /// </summary>
        /// <remarks>This enumeration defines how the drag ghost adjusts its size relative to the control
        /// or content being dragged: <list type="bullet"> <item> <term><see cref="FillControl"/></term>
        /// <description>The drag ghost resizes to fill the dimensions of the control being dragged.</description>
        /// </item> <item> <term><see cref="FitContent"/></term> <description>The drag ghost adjusts its size to fit the
        /// content being dragged, maintaining its natural dimensions.</description> </item> <item> <term><see
        /// cref="FixedWidth"/></term> <description>The drag ghost maintains a fixed width, regardless of the control or
        /// content being dragged.</description> </item> </list></remarks>
        public enum DragGhostSizeMode
        {
            FillControl,
            FitContent,
            FixedWidth
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
        /// Gets or sets a value indicating whether the focus rectangle should be drawn.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to draw the focus rectangle")]
        [DefaultValue(true)]
        public bool DrawFocusRectangle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether items can be moved using the Ctrl key combined with the Up or Down
        /// arrow keys.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Allows moving items with Ctrl+Up/Down arrow keys.")]
        [DefaultValue(false)]
        public bool AllowCtrlMoveitems { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether items can be reordered by dragging with the mouse.
        /// </summary>
        /// <remarks>When enabled, users can rearrange items interactively by dragging them with the
        /// mouse.  Ensure that the control supports drag-and-drop operations for this property to take
        /// effect.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Allows items to be reordered by dragging with the mouse")]
        [DefaultValue(false)]
        public bool AllowDragReorder { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the accent color is used for the drag reordering line.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use the border color or accent color for the drag reordering")]
        [DefaultValue(false)]
        public bool UseAccentForReorderLine { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to display a ghost representation of the selected item(s) during a
        /// drag operation.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to show the ghost of the selected item(s)")]
        [DefaultValue(false)]
        public bool ShowDragGhost { get; set; } = false;

        /// <summary>
        /// Gets or sets the opacity of the drag ghost when it is displayed.
        /// </summary>
        /// <remarks>The opacity value is clamped to the range of 0.1 to 1.0. Setting a value outside this
        /// range  will automatically adjust it to the nearest valid value.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Opacity of the drag ghost when shown")]
        [DefaultValue(0.5f)]
        public float DragGhostOpacity
        {
            get => _dragGhostOpacity;
            set 
            {
                float clamped = Math.Max(0.1f, Math.Min(value, 1f));
                _dragGhostOpacity = clamped; Invalidate(); 
            }
        }

        /// <summary>
        /// Gets or sets the mode that determines how the drag ghost width is calculated.
        /// </summary>
        /// <remarks>This property controls the appearance of the drag ghost by specifying how its width
        /// is determined.  Changing this property will cause the control to be redrawn.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Controls how the drag ghost width is determined.")]
        [DefaultValue(DragGhostSizeMode.FillControl)]
        public DragGhostSizeMode DragGhostMode
        {
            get => _dragGhostSizeMode;
            set { _dragGhostSizeMode = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the width, in pixels, of the drag ghost when <see cref="DragGhostMode"/> is set to
        /// <c>FixedWidth</c>.
        /// </summary>
        /// <remarks>Setting this property to a value less than 50 will automatically adjust it to 50. 
        /// Changes to this property will trigger a redraw of the control.</remarks>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("When DragGhostMode is FixedWidth, determines the ghost width in pixels.")]
        [DefaultValue(200)]
        public int DragGhostFixedWidth
        {
            get => _dragGhostFixedWidth;
            set { _dragGhostFixedWidth = Math.Max(50, value); Invalidate(); }
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        /// <summary>
        /// Gets the current search buffer used to store the most recent search query or input.
        /// </summary>
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

        /// <summary>
        /// Occurs when items in the list box are reordered, either moved up or down.
        /// </summary>
        /// <remarks>This event is triggered whenever the order of items in the list box changes. 
        /// Subscribers can use the event arguments to determine the details of the reordering operation.</remarks>
        [Category("BazthalLib - Behaviour")]
        [Description("Raised when items are moved up or down the list box")]
        public event EventHandler<ItemsReorderedEventArgs>? ItemsReordered;

        /// <summary>
        /// Occurs when the drag ghost needs to be drawn, allowing for full custom rendering.
        /// </summary>
        /// <remarks>This event is triggered during the rendering of the drag ghost, providing an
        /// opportunity  to customize its appearance. Subscribers can use the event arguments to access the  graphics
        /// context and item details for rendering.</remarks>
        [Category("BazthalLib - Appearance")]
        [Description("Raised when the drag ghost needs to be drawn. Allows full custom rendering.")]
        public event DrawItemEventHandler? DrawDragGhost;

        /// <summary>
        /// Occurs when files are dropped onto the list box.
        /// </summary>
        /// <remarks>This event is triggered when one or more files are dragged and dropped onto the list
        /// box. Subscribers can handle this event to process the dropped files.</remarks>
        [Category("BazthalLib - Behaviour")]
        [Description("Raised when files are dropped onto the list box")]
        public event EventHandler<FilesDroppedEventArgs>? FilesDropped;

        #endregion Events

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


        #endregion Constructor

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
        /// Handles key press events to provide custom keyboard navigation and selection behavior.
        /// </summary>
        /// <remarks>This method extends the default key handling behavior to support additional features:
        /// <list type="bullet"> <item> <description>If <see cref="AllowCtrlMoveitems"/> is <see langword="true"/> and
        /// the Control key is held, pressing the Up or Down arrow keys moves the selected items.</description> </item>
        /// <item> <description>If <see cref="EnableMultiSelect"/> is <see langword="true"/> and the Control key is
        /// held, pressing the 'A' key selects all items.</description> </item> <item> <description>Handles standard
        /// navigation keys (e.g., Up, Down, PageUp, PageDown, Home, End) to update the selection index.</description>
        /// </item> <item> <description>Raises the <see cref="EnterPressed"/> event when the Enter key is
        /// pressed.</description> </item> <item> <description>Raises the <see cref="EscapePressed"/> event when the
        /// Escape key is pressed.</description> </item> </list> When multi-selection is enabled, the Shift key allows
        /// range selection, and the Control key allows toggling individual selections.</remarks>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {


            if (AllowCtrlMoveitems && e.Control && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                MoveSelectedItems(e.KeyCode == Keys.Up ? -1 : 1);
                e.Handled = true;
                return;
            }

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
                    case Keys.Up: newIndex = Math.Max(0, newIndex - 1); break;
                    case Keys.Down: newIndex = Math.Min(Items.Count - 1, newIndex + 1); break;
                    case Keys.PageUp: newIndex = Math.Max(0, newIndex - visibleCount); break;
                    case Keys.PageDown: newIndex = Math.Min(Items.Count - 1, newIndex + visibleCount); break;
                    case Keys.Home: newIndex = 0; break;
                    case Keys.End: newIndex = Items.Count - 1; break;
                    case Keys.Enter: EnterPressed?.Invoke(this, EventArgs.Empty); e.Handled = true; return;
                    case Keys.Escape: EscapePressed?.Invoke(this, EventArgs.Empty); e.Handled = true; return;
                    default: return;
                }


                if (EnableMultiSelect)
                {
                    if (ModifierKeys.HasFlag(Keys.Shift) && _lastSelectedIndex != -1)
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
        /// Handles the mouse down event for the control, enabling focus, drag-and-drop reordering,  and item selection
        /// based on the mouse input and modifier keys.
        /// </summary>
        /// <remarks>This method supports drag-and-drop reordering if <see cref="AllowDragReorder"/> is
        /// enabled and the left mouse button is clicked once. It also manages item selection, supporting
        /// multi-selection when <see cref="EnableMultiSelect"/> is enabled.  The selection behavior depends on the
        /// state of the modifier keys: <list type="bullet"> <item><description>Holding the <see cref="Keys.Shift"/> key
        /// selects a range of items between the last selected index and the current index.</description></item>
        /// <item><description>Holding the <see cref="Keys.Control"/> key toggles the selection state of the clicked
        /// item.</description></item> <item><description>Clicking without modifier keys clears the current selection
        /// and selects the clicked item.</description></item> </list> If the click occurs outside the bounds of the
        /// selectable items, no action is taken.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data, including the mouse button, click count, and
        /// location.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();


            if (AllowDragReorder && e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                _dragStartPoint = e.Location;
                _dragPending = true;
            }


            if (e.X < GetContentWidth())
            {
                int index = ((e.Y - _topPadding) / _itemHeight) + _scrollValue;
                if (index < 0 || index >= Items.Count) return;


                if (EnableMultiSelect)
                {
                    if (ModifierKeys.HasFlag(Keys.Shift) && _lastSelectedIndex != -1)
                    {
                        _selectedIndices.Clear();
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
        /// Handles the <see cref="Control.MouseMove"/> event to manage drag-and-drop reordering of items.
        /// </summary>
        /// <remarks>This method initiates a drag-and-drop operation when the left mouse button is pressed
        /// and moved  outside the drag threshold while drag reordering is enabled. It updates the selected indices and 
        /// creates a visual drag ghost if configured to do so. <para> Drag-and-drop reordering is only performed if
        /// <see cref="AllowDragReorder"/> is <see langword="true"/>. </para></remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


            if (!AllowDragReorder || !_dragPending || e.Button != MouseButtons.Left)
                return;


            Rectangle dragBox = new Rectangle(
            _dragStartPoint.X - SystemInformation.DragSize.Width / 2,
            _dragStartPoint.Y - SystemInformation.DragSize.Height / 2,
            SystemInformation.DragSize.Width,
            SystemInformation.DragSize.Height);


            if (!dragBox.Contains(e.Location))
            {
                int indexPoint = IndexFromPoint(_dragStartPoint);
                if (indexPoint >= 0 && indexPoint < Items.Count)
                {
                    if (!_selectedIndices.Contains(indexPoint))
                    {
                        _selectedIndices.Clear();
                        _selectedIndices.Add(indexPoint);
                        _lastSelectedIndex = indexPoint;
                        Invalidate();
                        OnSelectedIndexChanged(EventArgs.Empty);
                    }


                    _dragIndices = _selectedIndices.OrderBy(i => i).ToList();
                    if (_dragIndices.Count > 0)
                    {
                        if (ShowDragGhost)
                        {
                            CreateDragGhost();

                            if (_dragGhost != null)
                            {
                                int verticalGap = 6; 
                                _ghostOffset = new Point(_dragGhost.Width / 2, _dragGhost.Height + verticalGap);
                            }
                        }

                        _dragPending = false;

                        var data = new DataObject();
                        data.SetData(ReorderFormat, _dragIndices.ToArray());
                        DoDragDrop(data, DragDropEffects.Move);

                    }

                }
            }
        }

        /// <summary>
        /// Handles the mouse button release event.
        /// </summary>
        /// <remarks>This method is called when a mouse button is released while the pointer is over the
        /// control.  It resets the drag operation state and ensures that the base class behavior is executed.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragPending = false;
        }

        /// <summary>
        /// Handles the mouse double-click event and updates the selected item in the control.
        /// </summary>
        /// <remarks>This method determines the item at the location of the double-click and updates the
        /// selection accordingly. If a valid item is double-clicked, the selection is cleared and set to the clicked
        /// item, and the  <see cref="OnSelectedIndexChanged"/> event is raised. The control is then invalidated to
        /// refresh its display.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data, including the mouse pointer location.</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int index = IndexFromPoint(e.Location);
            if (index >= 0 && index < Items.Count)
            {
                _selectedIndices.Clear();
                _selectedIndices.Add(index);
                _lastSelectedIndex = index;
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the painting of the control, including custom item rendering, scroll corner padding, drag-and-drop
        /// visuals, and other graphical elements.
        /// </summary>
        /// <remarks>This method is responsible for rendering the control's visual elements, such as
        /// items, borders, and drag-and-drop indicators. It also ensures proper handling of themes and graphical
        /// settings like smoothing and compositing modes. Derived classes can override this method to customize the
        /// painting behavior, but should call the base implementation to preserve default functionality.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the data for the <see cref="OnPaint"/> event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!SuppressDefaultItemDrawing || DrawItem != null)
                DrawItems(e.Graphics);

            // Draw corner pad exactly as before (crisp geometry)
            var corner = ScrollCornerRect;
            if (!corner.IsEmpty)
            {
                using var bg = new SolidBrush(_themeColors.BackColor);
                e.Graphics.FillRectangle(bg, corner);

                using var border = new Pen(_themeColors.BorderColor);
                var savedPO = e.Graphics.PixelOffsetMode;
                var savedSM = e.Graphics.SmoothingMode;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                e.Graphics.DrawRectangle(border,
                    new Rectangle(corner.X, corner.Y, corner.Width - 1, corner.Height - 1));

                e.Graphics.PixelOffsetMode = savedPO;
                e.Graphics.SmoothingMode = savedSM;
            }

            if (AllowDragReorder && ShowDragGhost && _dragGhost != null && _dragGhostOpacity > 0f)
            {
                var savedCM = e.Graphics.CompositingMode;
                var savedCQ = e.Graphics.CompositingQuality;
                var savedIM = e.Graphics.InterpolationMode;
                var savedPO = e.Graphics.PixelOffsetMode;
                var savedSM = e.Graphics.SmoothingMode;

                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                int x = _ghostPosition.X - _ghostOffset.X;
                int y = _ghostPosition.Y - _ghostOffset.Y;

                using var attr = new System.Drawing.Imaging.ImageAttributes();
                var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = _dragGhostOpacity };
                attr.SetColorMatrix(cm, System.Drawing.Imaging.ColorMatrixFlag.Default,
                                    System.Drawing.Imaging.ColorAdjustType.Bitmap);

                var destRect = new Rectangle(x, y, _dragGhost.Width, _dragGhost.Height);
                e.Graphics.DrawImage(_dragGhost, destRect, 0, 0,
                    _dragGhost.Width, _dragGhost.Height, GraphicsUnit.Pixel, attr);

                e.Graphics.CompositingMode = savedCM;
                e.Graphics.CompositingQuality = savedCQ;
                e.Graphics.InterpolationMode = savedIM;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            if (AllowDragReorder && _dropIndex >= 0)
            {
                using var pen = new Pen(UseAccentForReorderLine ? _themeColors.AccentColor : _themeColors.BorderColor, 2)
                { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };

                int lineY = (_dropIndex >= Items.Count)
                    ? _topPadding + ((Math.Min(Items.Count - 1, _scrollValue + VisibleItems - 1) - _scrollValue + 1) * _itemHeight)
                    : _topPadding + ((_dropIndex - _scrollValue) * _itemHeight);

                if (lineY >= _topPadding && lineY <= Height - _bottomPadding)
                    e.Graphics.DrawLine(pen, 0, lineY, Width, lineY);
            }

           
            DrawBorder(e.Graphics);

            int bottomPadding = (_hScrollBar?.Visible ?? false) ? _hScrollBar.Height + 2 : 4;
            int rightPadding = (_vScrollBar?.Visible ?? false) ? _vScrollBar.Width + 2 : 4;
           
            if (Focused && DrawFocusRectangle)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(2, 2, Width - rightPadding, Height - bottomPadding));
        }

        /// <summary>
        /// Handles the drag-and-drop operation when an object is dragged into the control's bounds.
        /// </summary>
        /// <remarks>This method determines the appropriate drag-and-drop effect based on the data being
        /// dragged: <list type="bullet"> <item> <description>If <see cref="AllowDragReorder"/> is <see
        /// langword="true"/> and the dragged data matches the reorder format, the effect is set to <see
        /// cref="DragDropEffects.Move"/>.</description> </item> <item> <description>If the dragged data contains file
        /// drop data, the effect is set to <see cref="DragDropEffects.Copy"/>.</description> </item> <item>
        /// <description>Otherwise, the effect is set to <see cref="DragDropEffects.None"/>.</description> </item>
        /// </list> The method also updates the drop index when reordering is allowed.</remarks>
        /// <param name="e">The <see cref="DragEventArgs"/> containing data about the drag event.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            if (AllowDragReorder && e.Data.GetDataPresent(ReorderFormat))
            {
                e.Effect = DragDropEffects.Move;
                UpdateDropIndex(e);
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                _dropIndex = -1;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

            base.OnDragEnter(e);
        }

        /// <summary>
        /// Handles the drag-over event to determine the appropriate drag-and-drop effect.
        /// </summary>
        /// <remarks>This method sets the drag-and-drop effect based on the type of data being dragged. If
        /// the data  matches the reorder format and drag reordering is allowed, the effect is set to <see
        /// cref="DragDropEffects.Move"/>.  If the data represents file drops, the effect is set to <see
        /// cref="DragDropEffects.Copy"/>.  Otherwise, the effect is set to <see
        /// cref="DragDropEffects.None"/>.</remarks>
        /// <param name="e">The <see cref="DragEventArgs"/> containing data about the drag event.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            if (AllowDragReorder && e.Data.GetDataPresent(ReorderFormat))
            {
                e.Effect = DragDropEffects.Move;
                UpdateDropIndex(e);
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

            base.OnDragOver(e);
        }

        /// <summary>
        /// Handles the drag-and-drop operation when an object is dropped onto the control.
        /// </summary>
        /// <remarks>This method supports two types of drag-and-drop operations: <list type="bullet">
        /// <item> <description>If <see cref="AllowDragReorder"/> is <see langword="true"/> and the dragged data matches
        /// the reorder format, the control performs a reorder operation.</description> </item> <item> <description>If
        /// the dragged data contains file paths (<see cref="DataFormats.FileDrop"/>), the <see cref="FilesDropped"/>
        /// event is raised with the dropped files.</description> </item> </list> If neither condition is met, the base
        /// class implementation is invoked.</remarks>
        /// <param name="e">The <see cref="DragEventArgs"/> containing data about the drag-and-drop operation.</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            if (AllowDragReorder && e.Data.GetDataPresent(ReorderFormat))
            {
                PerformReorder(e);
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FilesDropped?.Invoke(this, new FilesDroppedEventArgs(files));
            }

            base.OnDragDrop(e);
        }

        /// <summary>
        /// Handles the event when a drag operation leaves the control's bounds.
        /// </summary>
        /// <remarks>If drag reordering is enabled, this method resets the drop index and invalidates the
        /// control  to update its visual state. Always calls the base implementation to ensure standard
        /// behavior.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnDragLeave(EventArgs e)
        {
            if (AllowDragReorder)
            {
                _dropIndex = -1;
                Invalidate();
            }

            base.OnDragLeave(e);
        }

        #endregion Overrides

        #region Drawing

        /// <summary>
        /// Creates a visual representation of the items being dragged, known as a drag ghost.
        /// </summary>
        /// <remarks>The drag ghost is a bitmap that visually represents the selected items being dragged.
        /// Its size and appearance are determined by the current drag ghost size mode and the selected items. This
        /// method disposes of any previously created drag ghost before creating a new one.</remarks>
        private void CreateDragGhost()
        {
            if (_dragIndices == null || _dragIndices.Count == 0)
                return;
            int ghostWidth = Width;
            if (_dragGhostSizeMode == DragGhostSizeMode.FitContent)
            {
                int maxWidth = 0;
                foreach (var index in _dragIndices)
                {
                    string text = Items[index]?.ToString() ?? string.Empty;
                    int w = TextRenderer.MeasureText(text, Font).Width + 8;
                    maxWidth = Math.Max(maxWidth, w);
                }

                ghostWidth = maxWidth;
            }
            else if (_dragGhostSizeMode == DragGhostSizeMode.FixedWidth)
            {
                ghostWidth = _dragGhostFixedWidth;
            }

            int ghostHeight = _dragIndices.Count * _itemHeight;
            _dragGhost?.Dispose();
            _dragGhost = new Bitmap(ghostWidth, ghostHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(_dragGhost))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                for (int i = 0; i < _dragIndices.Count; i++)
                {
                    int index = _dragIndices[i];
                    var rect = new Rectangle(0, i * _itemHeight, ghostWidth, _itemHeight);
                    using var bg = new SolidBrush(_themeColors.SelectedItemBackColor);
                    using var fg = new SolidBrush(_themeColors.SelectedItemForeColor);
                    g.FillRectangle(bg, rect);
                    g.DrawString(Items[index]?.ToString() ?? string.Empty, Font, fg, rect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.NoWrap });
                }
            }
        }

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
        /// Updates the drop index based on the current drag event.
        /// </summary>
        /// <remarks>This method calculates the appropriate index for an item drop operation based on the
        /// mouse position during a drag-and-drop operation. It updates the internal drop index and ghost position, and
        /// triggers a redraw of the control.</remarks>
        /// <param name="e">The <see cref="DragEventArgs"/> containing the data for the drag event, including the current mouse
        /// position.</param>
        private void UpdateDropIndex(DragEventArgs e)
        {
            Point clientPoint = PointToClient(new Point(e.X, e.Y));
            _ghostPosition = clientPoint;
            int index = IndexFromPoint(clientPoint);
            _dropIndex = index < 0
                ? Items.Count
                : (clientPoint.Y > _topPadding + ((index - _scrollValue) * _itemHeight) + (_itemHeight / 2)
                    ? index + 1
                    : index);

            Invalidate();
            Update();
        }

        /// <summary>
        /// Reorders items in the collection based on the drag-and-drop operation.
        /// </summary>
        /// <remarks>This method adjusts the order of items in the collection by removing the dragged
        /// items from their original positions  and inserting them at the drop index. It updates the selected indices
        /// to reflect the new positions of the dragged items,  ensures the last selected item is visible, and raises
        /// the <see cref="ItemsReordered"/> event to notify listeners of the change.</remarks>
        /// <param name="e">The <see cref="DragEventArgs"/> containing data about the drag-and-drop operation.</param>
        private void PerformReorder(DragEventArgs e)
        {
            if (_dragIndices.Count == 0 || _dropIndex < 0) return;

            var oldIndices = _dragIndices.ToList();
            var draggedItems = _dragIndices.Select(i => Items[i]).ToList();

            foreach (var i in _dragIndices.OrderByDescending(i => i))
                Items.RemoveAt(i);

            int adjustedDrop = _dropIndex;
            foreach (var i in oldIndices)
                if (i < _dropIndex) adjustedDrop--;

            for (int j = 0; j < draggedItems.Count; j++)
                Items.Insert(adjustedDrop + j, draggedItems[j]);

            _selectedIndices.Clear();
            for (int j = 0; j < draggedItems.Count; j++)
                _selectedIndices.Add(adjustedDrop + j);

            _lastSelectedIndex = adjustedDrop;
            EnsureVisible(_lastSelectedIndex);
            Invalidate();
            OnSelectedIndexChanged(EventArgs.Empty);

            var newIndices = _selectedIndices.ToList();
            ItemsReordered?.Invoke(this, new ItemsReorderedEventArgs(oldIndices, newIndices));

            _dragGhost?.Dispose();
            _dragGhost = null;
            _dragIndices.Clear();
            _dropIndex = -1;
        }


        /// <summary>
        /// Moves the currently selected items in the list by the specified direction.
        /// </summary>
        /// <remarks>This method reorders the items in the list based on the current selection and the
        /// specified direction.  If the selected items are already at the boundary of the list (e.g., the first item
        /// for an upward move or the last item for a downward move),  the method does nothing. After the move, the
        /// selection is updated to reflect the new positions of the moved items.</remarks>
        /// <param name="direction">The direction to move the selected items. A negative value moves the items up, and a positive value moves
        /// them down.</param>
        private void MoveSelectedItems(int direction)
        {
            try
            {
                if (_selectedIndices.Count == 0 || Items.Count < 2) return;

                var oldIndices = _selectedIndices.ToList();

                var indices = direction < 0 ? _selectedIndices.OrderBy(i => i).ToList() : _selectedIndices.OrderByDescending(i => i).ToList();

                if ((direction < 0 && indices.First() == 0) || (direction > 0 && indices.First() == Items.Count - 1)) return;


                foreach (var index in indices)
                {
                    int newIndex = index + direction;
                    var temp = Items[index];
                    Items[index] = Items[newIndex];
                    Items[newIndex] = temp;
                }

                _selectedIndices.Clear();
                foreach (var index in indices)
                    _selectedIndices.Add(index + direction);

                _lastSelectedIndex = direction < 0 ? _selectedIndices.Min() : _selectedIndices.Max();

                EnsureVisible(_lastSelectedIndex);
                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);

                var newIndices = _selectedIndices.ToList();
                ItemsReordered?.Invoke(this, new ItemsReorderedEventArgs(oldIndices, newIndices));
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Move Selected Items", "ThemableListBox", ex.Message);
            }


        }

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
