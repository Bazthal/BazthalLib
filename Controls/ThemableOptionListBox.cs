using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BazthalLib.Controls
{
    /// <summary>
    /// Specifies the visual representation of a selection control.
    /// </summary>
    /// <remarks>This enumeration is used to define the type of visual element that represents a selection
    /// option, such as a radio button or a checkbox. It can be used to customize the appearance of selection controls
    /// in a user interface.</remarks>
    public enum SelectionVisual
    {
        None,
        Radio,
        Checkbox
    }

    public class ThemableOptionListBox : ThemableListBox
    {
        #region Fields
        private string _version = "V1.0";
        private SelectionVisual _selectionStyle = SelectionVisual.None;
        private int _selectedRadioIndex = -1;
        private HashSet<int> _checkedIndices = new();
        private bool _allowSelectionHighlight = false;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets the unique identifier for the control, incorporating the version information.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new string ControlID => $"ThemableOptionListBox {_version}";

        /// <summary>
        /// Gets or sets the visual selection style for the control.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("The visual selection style: None, Radio, or Checkbox.")]
        [DefaultValue(SelectionVisual.None)]
        public SelectionVisual SelectionStyle
        {
            get => _selectionStyle;
            set { _selectionStyle = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the highlight is drawn on the selected item.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to draw the highlight on the selected item or not")]
        [DefaultValue(false)]
        public bool AllowSelectionHighlight
        {
            get => _allowSelectionHighlight;
            set { _allowSelectionHighlight = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the index of the currently selected radio button.
        /// </summary>
        /// <remarks>Setting this property to a value outside the range of available items will have no
        /// effect.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedRadioIndex
        {
            get => _selectedRadioIndex;
            set
            {
                if (_selectedRadioIndex != value && value >= -1 && value < Items.Count)
                {
                    _selectedRadioIndex = value;
                    Invalidate();
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the set of indices that are currently checked.
        /// </summary>
        [Browsable(false)]
        public HashSet<int> CheckedIndices => _checkedIndices;

        /// <summary>
        /// Gets the currently selected radio item from the list of items.
        /// </summary>
        [Browsable(false)]
        public object? SelectedRadioItem =>
    (SelectionStyle == SelectionVisual.Radio && SelectedRadioIndex >= 0 && SelectedRadioIndex < Items.Count)
    ? Items[SelectedRadioIndex]
    : null;

        /// <summary>
        /// Gets the collection of items that are currently checked.
        /// </summary>
        /// <remarks>The collection is determined based on the current <see cref="SelectionStyle"/>.  If
        /// the selection style is set to <see cref="SelectionVisual.Checkbox"/>,  it returns the items corresponding to
        /// the checked indices.  Otherwise, it returns an empty collection.</remarks>
        [Browsable(false)]
        public IEnumerable<object> CheckedItems =>
            SelectionStyle == SelectionVisual.Checkbox
            ? CheckedIndices.Where(i => i >= 0 && i < Items.Count).Select(i => Items[i])
            : Enumerable.Empty<object>();

        #endregion Properties

        #region Contstructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableOptionListBox"/> class with default settings.
        /// </summary>
        /// <remarks>This constructor sets up the control with optimized double buffering and redraw
        /// styles to enhance performance and visual appearance. It also initializes the item height and subscribes to
        /// the <see cref="DrawItem"/> event to handle custom item drawing.</remarks>
        public ThemableOptionListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SuppressDefaultItemDrawing = false;
            ItemHeight = 17;
            DoubleBuffered = true;

            DrawItem += OnDrawOptionItem;
        }
        #endregion Contstructor

        #region Drawing
        /// <summary>
        /// Handles the drawing of an option item in a custom list control.
        /// </summary>
        /// <remarks>This method customizes the appearance of list items based on the <see
        /// cref="SelectionStyle"/> property. It supports different visual styles such as radio buttons, checkboxes, and
        /// standard list items.</remarks>
        /// <param name="sender">The source of the event, typically the control that owns the items.</param>
        /// <param name="e">A <see cref="DrawItemEventArgs"/> that contains the event data, including the index of the item to be drawn
        /// and the graphics surface to draw on.</param>
        private void OnDrawOptionItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;

            switch (SelectionStyle)
            {
                case SelectionVisual.Radio:
                    DrawRadioItem(e);
                    break;
                case SelectionVisual.Checkbox:
                    DrawCheckboxItem(e);
                    break;
                default:
                    DrawStandardItem(e);
                    break;
            }
        }

        /// <summary>
        /// Draws a standard item in the control, applying custom styles for selection and hover states.
        /// </summary>
        /// <remarks>This method customizes the appearance of items in the control by changing the
        /// background and text colors based on the item's selection and hover states. The method uses the <see
        /// cref="SelectedItemBackColor"/> and <see cref="SelectedItemForeColor"/> properties for selected or hovered
        /// items, and the default <see cref="BackColor"/> and <see cref="ForeColor"/> for other items. It also ensures
        /// that the text is vertically centered and left-aligned within the item bounds.</remarks>
        /// <param name="e">The <see cref="DrawItemEventArgs"/> containing data for the item to be drawn, including graphics context and
        /// item bounds.</param>
        private void DrawStandardItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            string text = Items[e.Index]?.ToString() ?? string.Empty;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected && _allowSelectionHighlight;

            bool isHovered = e.Index == HoverIndex && AllowHoverHighlight;

            Color backColor = isSelected || isHovered ? SelectedItemBackColor : BackColor;
            Color textColor = isSelected || isHovered ? SelectedItemForeColor : ForeColor;


            using Brush bg = new SolidBrush(backColor);
            g.FillRectangle(bg, new Rectangle(0, e.Bounds.Top, this.Width, e.Bounds.Height));

            TextRenderer.DrawText(g, text, Font, e.Bounds, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Draws a radio button item within a specified area, applying visual styles based on its state.
        /// </summary>
        /// <remarks>This method customizes the appearance of a radio button item, including its
        /// background, border, and text, based on whether it is selected, hovered, or in its default state. It uses the
        /// provided graphics context to render the item within the specified bounds.</remarks>
        /// <param name="e">The <see cref="DrawItemEventArgs"/> containing data for the item to be drawn, including graphics context and
        /// item bounds.</param>
        private void DrawRadioItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            string text = Items[e.Index]?.ToString() ?? string.Empty;
            bool isSelectedRadio = e.Index == SelectedRadioIndex;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected && _allowSelectionHighlight;
            bool isHovered = e.Index == HoverIndex && AllowHoverHighlight;

            int padding = 4;
            int radioSize = e.Bounds.Height - 6;
            Rectangle dotRect = new(e.Bounds.Left + padding, e.Bounds.Top + 4, radioSize, radioSize);
            Rectangle textRect = new(dotRect.Right + padding, e.Bounds.Top, e.Bounds.Right - dotRect.Right - padding, e.Bounds.Height);

            Color backColor = isSelected || isHovered ? SelectedItemBackColor : BackColor;
            Color textColor = isSelected || isHovered ? SelectedItemForeColor : ForeColor;

            using Brush bg = new SolidBrush(backColor);
            g.FillRectangle(bg, new Rectangle(0, e.Bounds.Top, this.Width, e.Bounds.Height));


            using Pen borderPen = new(BorderColor);
            g.DrawEllipse(borderPen, dotRect);

            if (isSelectedRadio)
            {
                Rectangle inner = Rectangle.Inflate(dotRect, -3, -3);
                using Brush fill = new SolidBrush(AccentColor);
                g.FillEllipse(fill, inner);
            }

            TextRenderer.DrawText(g, text, Font, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Draws a checkbox item within a specified area, including its text and selection state.
        /// </summary>
        /// <remarks>This method handles the rendering of a checkbox item, including its background,
        /// border, and text. It considers the item's checked state, selection state, and hover state to determine the
        /// appropriate colors and styles. The method also ensures that the focus rectangle is drawn if the item is
        /// focused.</remarks>
        /// <param name="e">The <see cref="DrawItemEventArgs"/> containing the data needed for drawing the item.</param>
        private void DrawCheckboxItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            string text = Items[e.Index]?.ToString() ?? string.Empty;
            bool isChecked = CheckedIndices.Contains(e.Index);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected && _allowSelectionHighlight;
            bool isHovered = e.Index == HoverIndex && AllowHoverHighlight;


            int padding = 4;
            int boxSize = 15;
            Rectangle checkboxRect = new(e.Bounds.Left + padding, e.Bounds.Top + (e.Bounds.Height - boxSize) / 2, boxSize, boxSize);
            Rectangle textRect = new(checkboxRect.Right + padding, e.Bounds.Top, e.Bounds.Right - checkboxRect.Right - padding, e.Bounds.Height);

            Color backColor = isSelected || isHovered ? SelectedItemBackColor : BackColor;
            Color textColor = isSelected || isHovered ? SelectedItemForeColor : ForeColor;

            using Brush bg = new SolidBrush(backColor);
            g.FillRectangle(bg, new Rectangle(0, e.Bounds.Top, this.Width, e.Bounds.Height));


            using Pen pen = new(BorderColor);
            g.DrawRectangle(pen, checkboxRect);

            if (isChecked)
            {
                Rectangle fill = Rectangle.Inflate(checkboxRect, -3, -3);
                using Brush fillBrush = new SolidBrush(AccentColor);
                g.FillRectangle(fillBrush, fill);
            }

            TextRenderer.DrawText(g, text, Font, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }
        #endregion Drawing

        #region Mouse Interaction
        /// <summary>
        /// Handles the mouse down event for the control, updating the selection based on the current selection style.
        /// </summary>
        /// <remarks>This method updates the selection of items in the control when the mouse button is
        /// pressed. If the <see cref="SelectionStyle"/> is set to <see cref="SelectionVisual.Radio"/>, the selected
        /// index is updated to the item at the mouse location. If the <see cref="SelectionStyle"/> is set to <see
        /// cref="SelectionVisual.Checkbox"/>, the checked state of the item at the mouse location is toggled.</remarks>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int index = IndexFromPoint(e.Location);
            if (index < 0 || index >= Items.Count) return;

            switch (SelectionStyle)
            {
                case SelectionVisual.Radio:
                    SelectedRadioIndex = index;
                    break;
                case SelectionVisual.Checkbox:
                    if (CheckedIndices.Contains(index))
                        CheckedIndices.Remove(index);
                    else
                        CheckedIndices.Add(index);
                    Invalidate();
                    break;
            }
        }
        #endregion Mouse Interaction
    }
}
