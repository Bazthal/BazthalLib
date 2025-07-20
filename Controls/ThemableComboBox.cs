using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BazthalLib.UI;


namespace BazthalLib.Controls
{
    [ToolboxItem(true)]
    [DefaultProperty("Items")]
    [DesignerCategory("Code")]
    [DefaultEvent("SelectedIndexChanged")]
    public class ThemableComboBox : ThemableControlBase
    {
        #region Fields

        private string _version = "V1.1";
        private ThemableListBox _dropdownList;
        private Form _popupHost;
        private NotifyingItemCollection _items;
        private ListBox.ObjectCollection _baseItems;
        private readonly ListBox _hiddenListBox = new();
        private bool _isDropdownVisible = false;
        private Rectangle _dropDownButtonBounds;
        private int _itemHeight = 17;
        private int _bufferHeight = 3;
        private int _selectedIndex = -1;
        private string _selectedItem;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the unique identifier for the ThemableComboBox control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string ControlID => $"ThemableComboBox {_version}";

        /// <summary>
        /// Gets or sets the height of each item in the dropdown list.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Item height for the items in the dropdown list")]
        [DefaultValue(17)]
        public int ItemHeight
        {
            get => _itemHeight; set { _itemHeight = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the buffer height for the items in the dropdown list.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Buffer height for the items in the dropdown list")]
        [DefaultValue(3)]
        public int BufferHeight
        {
            get => _bufferHeight; set { _bufferHeight = value; Invalidate(); }
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
        /// Gets the collection of items displayed in the list.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Data")]
        [Description("The items displayed in the list.")]
        public NotifyingItemCollection Items => _items;

        /// <summary>
        /// Gets or sets the index of the currently selected item in the list.
        /// </summary>
        /// <remarks>Setting this property to a value less than -1 or greater than or equal to the number
        /// of items in the list will have no effect. When the selected index changes, the <see
        /// cref="OnSelectedIndexChanged"/> event is raised.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value == _selectedIndex || value < -1 || value >= Items.Count) return;

                _selectedIndex = value;
                _selectedItem = value >= 0 ? Items[value]?.ToString() : null;

                if (_dropdownList != null)
                    _dropdownList.SelectedIndex = _selectedIndex;


                Invalidate();
                OnSelectedIndexChanged(EventArgs.Empty);

            }
        }

        /// <summary>
        /// Gets or sets the currently selected item in the list.
        /// </summary>
        /// <remarks>Changing the selected item will update the selected index and trigger the <see
        /// cref="OnSelectedIndexChanged"/> event.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    int newIndex = Items.IndexOf(value);
                    if (newIndex != _selectedIndex)
                        _selectedIndex = newIndex;
                    Invalidate();
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableComboBox"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets up the <see cref="ThemableComboBox"/> with specific control
        /// styles for optimized painting and user interaction. It also initializes the control's dimensions and item
        /// collections.</remarks>
        #region Contstructor
        public ThemableComboBox()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer, true);
            this.TabStop = true;
            this.Height = _itemHeight + 6;
            this.Width = 121; //Same as default ComboBox width
            _baseItems = _hiddenListBox.Items;
            _items = new NotifyingItemCollection(_baseItems, this);
        }
        #endregion Contstructor

        #region Events
        /// <summary>
        /// Occurs when the selected index changes.
        /// </summary>
        /// <remarks>This event is raised whenever the selection changes, allowing subscribers to respond
        /// to the change.</remarks>
        public event EventHandler SelectedIndexChanged;
        /// <summary>
        /// Raises the <see cref="SelectedIndexChanged"/> event.
        /// </summary>
        /// <remarks>This method is called whenever the selected index changes. Derived classes can
        /// override this method to handle the event without attaching a delegate. It is recommended to call the base
        /// class's <see cref="OnSelectedIndexChanged"/> method to ensure that registered delegates receive the
        /// event.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }
        #endregion Events

        #region Input Handling
        /// <summary>
        /// Handles the <see cref="Control.KeyDown"/> event to manage dropdown visibility and navigation.
        /// </summary>
        /// <remarks>This method shows the dropdown when the down or up arrow keys are pressed and the
        /// dropdown is not visible. If the dropdown is visible, it navigates through the items using the arrow keys,
        /// selects an item with the Enter key, or hides the dropdown with the Escape key. The event is marked as
        /// handled if any of these actions are performed.</remarks>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_isDropdownVisible && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
            {
                ShowDropdown();
                e.Handled = true;
            }
            else if (_isDropdownVisible)
            {
                if (e.KeyCode == Keys.Down && _dropdownList.SelectedIndex < _dropdownList.Items.Count - 1)
                {
                    _dropdownList.SelectedIndex++;
                    _dropdownList.TopIndex = Math.Min(_dropdownList.SelectedIndex, _dropdownList.Items.Count - 1);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up && _dropdownList.SelectedIndex > 0)
                {
                    _dropdownList.SelectedIndex--;
                    _dropdownList.TopIndex = Math.Max(0, _dropdownList.SelectedIndex);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    if (_dropdownList.SelectedItem != null)
                        SelectedItem = _dropdownList.SelectedItem.ToString();

                    HideDropdown();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    HideDropdown();
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// Determines whether the specified key is a regular input key or a special key that requires preprocessing.
        /// </summary>
        /// <remarks>This method is typically overridden to specify which keys are considered input keys
        /// that should be processed directly by the control.</remarks>
        /// <param name="keyData">The key data to evaluate, typically a value from the <see cref="Keys"/> enumeration.</param>
        /// <returns><see langword="true"/> if the specified key is <see cref="Keys.Up"/>, <see cref="Keys.Down"/>, <see
        /// cref="Keys.Enter"/>, or <see cref="Keys.Escape"/>; otherwise, the result of the base class implementation.</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Enter || keyData == Keys.Escape)
                return true;

            return base.IsInputKey(keyData);
        }
        /// <summary>
        /// Handles the click event for the control, setting focus and toggling the dropdown state.
        /// </summary>
        /// <remarks>This method ensures that the control receives focus when clicked and toggles the
        /// dropdown state. It overrides the base <see cref="OnClick"/> method to provide additional functionality
        /// specific to this control.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.Focus(); // Ensure the control gets focus when clicked
            ToggleDropdown();
        }
        /// <summary>
        /// Handles the mouse wheel event to change the selected index in the control.
        /// </summary>
        /// <remarks>This method adjusts the selected index based on the direction of the mouse wheel
        /// scroll. Scrolling up increases the index, while scrolling down decreases it. The index is clamped between 0
        /// and the number of items minus one. If the index changes, the control is redrawn and the <see
        /// cref="OnSelectedIndexChanged"/> event is triggered.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int oldIndex = SelectedIndex;

            // Adjust index by ±1
            int newIndex = SelectedIndex - Math.Sign(e.Delta);

            // Clamp between 0 and _items.Count - 1 (no -1 allowed)
            newIndex = Math.Max(0, Math.Min(_items.Count - 1, newIndex));

            if (newIndex != oldIndex)
            {
                SelectedIndex = newIndex; // triggers OnSelectedIndexChanged
                DebugUtils.Log("ComboBox", "Index Changed", $"New index: {SelectedIndex}");
                Invalidate(); // redraw control if necessary
            }
        }


        #endregion Input Handling

        #region Focus Handling
        /// <summary>
        /// Handles the event when the control gains focus.
        /// </summary>
        /// <remarks>This method is overridden to perform additional actions when the control receives
        /// focus, such as logging the event and invalidating the control to redraw the focus rectangle.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            DebugUtils.Log("OnGotFocus", "ThemableComboBox", "Showing dropdown on focus.");
            Invalidate(); // Redraw for focus rectangle
        }

        /// <summary>
        /// Handles the event when the control loses focus.
        /// </summary>
        /// <remarks>This method hides the dropdown list if neither the control nor the dropdown list has
        /// focus. It also invalidates the control to trigger a repaint.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DebugUtils.Log("OnLostFocus", "ThemableComboBox", "Hiding dropdown on lost focus.");
            BeginInvoke(new Action(() =>
            {
                if (!this.Focused && (_dropdownList == null || !_dropdownList.Focused))
                {
                    HideDropdown();
                }
            }));
            Invalidate();
        }
        #endregion Focus Handling

        #region Drawing and DropDown
        /// <summary>
        /// Toggles the visibility of the dropdown menu.
        /// </summary>
        /// <remarks>If the dropdown menu is currently visible, this method hides it.  Otherwise, it
        /// displays the dropdown menu.</remarks>
        private void ToggleDropdown()
        {
            if (_isDropdownVisible)
                HideDropdown();
            else
                ShowDropdown();
        }
        /// <summary>
        /// Displays a dropdown list below the current control, allowing the user to select an item from the list.
        /// </summary>
        /// <remarks>The dropdown is initialized with a list of items and positioned based on available
        /// screen space. It handles user interactions such as item selection and keyboard navigation (Enter/Escape
        /// keys). The dropdown is hidden when an item is selected or when the dropdown loses focus.</remarks>
        private void ShowDropdown()
        {
            if (_dropdownList == null)
            {
                _dropdownList = new ThemableListBox
                {
                    Height = Math.Min(180, _items.Count * _itemHeight + _bufferHeight),
                    Width = this.Width,
                    EnableHorizontalScroll = false,
                    ItemHeight = _itemHeight
                };

                // Hook up click selection
                _dropdownList.Click += (s, e) =>
                {
                    if (_dropdownList.SelectedItem != null)
                        SelectedIndex = _dropdownList.SelectedIndex;

                    HideDropdown();
                };

                // Handle Enter/Escape from list box
                _dropdownList.EnterPressed += (s, e) =>
                {
                    SelectedIndex = _dropdownList.SelectedIndex;
                    HideDropdown();
                };
                _dropdownList.EscapePressed += (s, e) => HideDropdown();
            }

            // Initialize popup host form
            if (_popupHost == null)
            {
                _popupHost = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    TopMost = true,
                    BackColor = _dropdownList.BackColor,
                    AutoSize = false
                };

                // Override key processing (backup for Enter/Escape)
                _popupHost.KeyPreview = true;
                _popupHost.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SelectedIndex = _dropdownList.SelectedIndex;
                        HideDropdown();
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Escape)
                    {
                        HideDropdown();
                        e.Handled = true;
                    }
                };

                Theming.RegisterForm(_popupHost);
                _popupHost.Deactivate += (s, e) => HideDropdown();
                _popupHost.Controls.Add(_dropdownList);
            }

            // Populate items
            _dropdownList.Items.Clear();
            foreach (var item in _items)
                _dropdownList.Items.Add(item);
            _dropdownList.SelectedIndex = _items.IndexOf(_selectedItem);

            // Positioning
            Point screenPos = this.PointToScreen(Point.Empty);
            Rectangle workingArea = Screen.FromPoint(screenPos).WorkingArea;

            int dropdownHeight = _dropdownList.Height;
            int spaceBelow = workingArea.Bottom - (screenPos.Y + Height);
            int spaceAbove = screenPos.Y;

            Point location;

            if (spaceBelow >= dropdownHeight)
            {
                location = new Point(screenPos.X, screenPos.Y + Height);
            }
            else if (spaceAbove >= dropdownHeight)
            {
                location = new Point(screenPos.X, screenPos.Y - dropdownHeight);
            }
            else if (spaceBelow >= spaceAbove)
            {
                dropdownHeight = spaceBelow;
                location = new Point(screenPos.X, screenPos.Y + Height);
            }
            else
            {
                dropdownHeight = spaceAbove;
                location = new Point(screenPos.X, screenPos.Y - dropdownHeight);
            }

            _dropdownList.Height = dropdownHeight;
            _popupHost.Size = _dropdownList.Size;
            _popupHost.Location = location;

            // Show and focus
            _popupHost.Show();
            _popupHost.BringToFront();
            _dropdownList.Focus();

            _isDropdownVisible = true;
        }

        /// <summary>
        /// Hides the dropdown menu if it is currently visible.
        /// </summary>
        /// <remarks>This method sets the dropdown visibility state to hidden and ensures that the
        /// associated popup host is not displayed.</remarks>
        private void HideDropdown()
        {
            _popupHost?.Hide();
            _isDropdownVisible = false;
        }

        /// <summary>
        /// Handles the painting of the control, including the background, selected text, focus rectangle, and dropdown
        /// button.
        /// </summary>
        /// <remarks>This method customizes the appearance of the control by drawing the background, the
        /// selected text, and a dropdown button. If the control is focused, a focus rectangle is also drawn. The
        /// dropdown button is represented by a polygon drawn at the right side of the control.</remarks>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Background
            using SolidBrush backBrush = new(BackColor);
            g.FillRectangle(backBrush, this.ClientRectangle);

            
            // Selected Text
            string text = _selectedItem?.ToString() ?? "";
            // TextRenderer.DrawText(g, text, Font, new Point(4, 4), ForeColor);
            var textRect = new Rectangle(4, 4, Width - 22, Height - 8);
            TextRenderer.DrawText(g, text, Font, textRect, ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            if (Focused)
            {
                ControlPaint.DrawFocusRectangle(g, new Rectangle(2, 2, Width - 4, Height - 4));
            }


            // Dropdown button
            _dropDownButtonBounds = new Rectangle(Width - 18, 4, 14, Height - 8);
            Point[] arrow = {
                new Point(_dropDownButtonBounds.Left + 2, _dropDownButtonBounds.Top + 5),
                new Point(_dropDownButtonBounds.Right - 2, _dropDownButtonBounds.Top + 5),
                new Point(_dropDownButtonBounds.Left + 7, _dropDownButtonBounds.Bottom - 5)
            };
            g.FillPolygon(new SolidBrush(AccentColor), arrow);

            base.OnPaint(e);
        }
        #endregion Drawing and DropDown

        #region Disposal
        /// <summary>
        /// Releases the unmanaged resources used by the component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _dropdownList != null)
            {
                _dropdownList.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion Disposal
    }
}
