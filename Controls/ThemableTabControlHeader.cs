using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    [DefaultProperty(nameof(LinkedTabControlName))]
    [DefaultEvent("Click")]
    public class ThemableTabControlHeader : FlowLayoutPanel, IThemableControl
    {
        #region Fields and Properties
        private string _version = "V1.0";
        private Color _underlineColor = SystemColors.ActiveBorder;
        private TabControl _linkedTabControl;
        private string _linkedTabControlName = "";
        private bool _matchBorderColor = false;
        private bool _showButtonBorder = false;
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();

        /// <summary>
        /// Gets the unique identifier for the ThemableTabControlHeader.
        /// </summary>
        [Category("BazthalLib")]
        [Description("This has been added by the BazhalLib Library")]
        [ReadOnly(true)]
        public string ControlID => $"ThemableTabControlHeader {_version}";

        /// <summary>
        /// Gets or sets a value indicating whether theme colors are used for the appearance.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to use theme colors or not.")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the accent color or the border color is used as the highlighted
        /// button color.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("Whether to use the accent color or the border color as the highlighted button")]
        [DefaultValue(false)]
        public bool MatchBorderColor
        {
            get => _matchBorderColor;
            set
            {
                if (_matchBorderColor != value)
                {
                    _matchBorderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a border is displayed around the buttons.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether to show a border around the buttons.")]
        [DefaultValue(false)]
        public bool ShowButtonBorder
        {
            get => _showButtonBorder;
            set
            {
                if (_showButtonBorder != value)
                {
                    _showButtonBorder = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the linked TabControl.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Behavior")]
        [Description("The name of the TabControl to link to.")]
        [DefaultValue("")]
        [TypeConverter(typeof(TabControlNameConverter))]
        public string LinkedTabControlName
        {
            get => _linkedTabControlName;
            set
            {
                if (_linkedTabControlName != value)
                {
                    _linkedTabControlName = value;
                    TryResolveTabControl();

                    if (IsInDesignMode())
                    {
                        Invalidate();
                        Refresh();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="TabControl"/> that is linked to this component.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabControl LinkedTabControl
        {
            get => _linkedTabControl;
            private set
            {
                if (_linkedTabControl != null)
                {
                    _linkedTabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
                    _linkedTabControl.ControlAdded -= TabControl_ControlChanged;
                    _linkedTabControl.ControlRemoved -= TabControl_ControlChanged;
                }

                _linkedTabControl = value;

                if (_linkedTabControl != null)
                {
                    _linkedTabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                    _linkedTabControl.ControlAdded += TabControl_ControlChanged;
                    _linkedTabControl.ControlRemoved += TabControl_ControlChanged;
                }

                GenerateButtons();

                Invalidate();
                Refresh();
            }
        }

        /// <summary>
        /// Determines whether the component is currently in design mode.
        /// </summary>
        /// <returns><see langword="true"/> if the component is in design mode; otherwise, <see langword="false"/>.</returns>
        private bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;
        }


        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableTabControlHeader"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the control with optimized double buffering and resize
        /// redrawing for improved performance. It also configures the control to automatically size itself and prevents
        /// wrapping of its contents.</remarks>
        public ThemableTabControlHeader()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(5);
            WrapContents = false;
        }

        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Raises the <see cref="System.Windows.Forms.Control.CreateControl"/> event.
        /// </summary>
        /// <remarks>This method ensures that the control is properly initialized and attempts to resolve
        /// the tab control. It is called when the control is first created.</remarks>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TryResolveTabControl();
        }

        /// <summary>
        /// Handles the event when the parent control of this control changes.
        /// </summary>
        /// <remarks>This method attaches event handlers to the <see cref="Control.ControlAdded"/> and 
        /// <see cref="Control.ControlRemoved"/> events of the new parent control, if it is not null.</remarks>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
            {
                Parent.ControlAdded += Parent_ControlChanged;
                Parent.ControlRemoved += Parent_ControlChanged;
            }
        }

        /// <summary>
        /// Handles the event when a control is added to or removed from the parent control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the parent control.</param>
        /// <param name="e">The event data containing information about the control that was changed.</param>
        private void Parent_ControlChanged(object sender, ControlEventArgs e)
        {
            TryResolveTabControl();
        }

        /// <summary>
        /// Attempts to resolve and assign a <see cref="TabControl"/> to the <c>LinkedTabControl</c> property.
        /// </summary>
        /// <remarks>This method searches for a <see cref="TabControl"/> within the parent form's
        /// controls.  If a control with the name specified in <c>_linkedTabControlName</c> is found, it is assigned to 
        /// <c>LinkedTabControl</c>. If no named control is found, the first <see cref="TabControl"/>  encountered in
        /// the control hierarchy is assigned.</remarks>
        private void TryResolveTabControl()
        {

            var parent = FindForm();
            if (parent == null) return;

            if (!string.IsNullOrEmpty(_linkedTabControlName))
            {
                var named = parent.Controls.Find(_linkedTabControlName, true).FirstOrDefault() as TabControl;
                if (named != null)
                {
                    LinkedTabControl = named;
                    return;
                }
            }

            var firstTab = parent.Controls.OfType<Control>()
                .SelectMany(c => GetAllControlsRecursive(c))
                .OfType<TabControl>()
                .FirstOrDefault();

            if (firstTab != null)
            {
                LinkedTabControl = firstTab;
            }
        }

        /// <summary>
        /// Retrieves all child controls of the specified root control, including nested children, in a flat array.
        /// </summary>
        /// <param name="root">The root control from which to retrieve all descendant controls.</param>
        /// <returns>An array of <see cref="Control"/> objects representing all descendant controls of the specified root
        /// control.</returns>
        private static Control[] GetAllControlsRecursive(Control root)
        {
            return root.Controls
                       .Cast<Control>()
                       .SelectMany(ctrl => new[] { ctrl }.Concat(GetAllControlsRecursive(ctrl)))
                       .ToArray();
        }

        /// <summary>
        /// Handles the event when the selected tab in the tab control changes.
        /// </summary>
        /// <param name="sender">The source of the event, typically the tab control.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            HighlightSelected();
        }
        /// <summary>
        /// Handles the event when a control is added to or removed from the tab control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the tab control.</param>
        /// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
        private void TabControl_ControlChanged(object sender, ControlEventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => GenerateButtons()));
        }

        /// <summary>
        /// Generates and adds buttons corresponding to each tab page in the linked <see cref="TabControl"/>.
        /// </summary>
        /// <remarks>This method clears any existing buttons and creates a new button for each tab page in
        /// the linked <see cref="TabControl"/>. Each button is styled according to the current theme and settings, and
        /// clicking a button will trigger the <see cref="TabButton_Click"/> event handler. The method also highlights
        /// the currently selected tab and refreshes the control to reflect changes.</remarks>
        private void GenerateButtons()
        {
            if (_linkedTabControl == null) return; // Ensure we have a linked TabControl 

            // Clear existing buttons
            Controls.Clear();

            // Generate buttons for each tab page
            for (int i = 0; i < _linkedTabControl.TabPages.Count; i++)
            {
                var tabPage = _linkedTabControl.TabPages[i];
                var btn = new ThemableButton
                {
                    Text = tabPage.Text,
                    Tag = i,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(4),
                    AutoSize = true,
                    EnableBorder = _showButtonBorder,
                    AllowFocus = false, // Disable focus
                    AllowClickHighlight = false // Disable Click Highlight since we handle selection highlighting with a custom underline
                };

                btn.FlatAppearance.BorderSize = 0;

                btn.Click += TabButton_Click;
                //btn.Paint += Button_Paint;
                Controls.Add(btn);
            }

            HighlightSelected();
            Invalidate();
            Refresh();
        }

        /// <summary>
        /// Handles the click event for a tab button, selecting the corresponding tab in the linked tab control.
        /// </summary>
        /// <remarks>This method updates the selected tab in the linked tab control based on the tab index
        /// specified in the button's <c>Tag</c> property. If the application is in design mode, it also updates the
        /// selection in the designer to reflect the selected tab.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="ThemableButton"/> with a valid tab index in its
        /// <c>Tag</c> property.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void TabButton_Click(object? sender, EventArgs e)
        {

            if (sender is not ThemableButton btn || btn.Tag is not int tabIndex || _linkedTabControl == null)
                return;

            if (tabIndex < 0 || tabIndex >= _linkedTabControl.TabPages.Count)
                return;

            _linkedTabControl.SelectedIndex = tabIndex;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime && Site != null)
            {
                var host = (IDesignerHost)Site.GetService(typeof(IDesignerHost));
                var selectionService = (ISelectionService)Site.GetService(typeof(ISelectionService));

                if (host != null && selectionService != null)
                {

                    selectionService.SetSelectedComponents(
                        new object[] { _linkedTabControl.TabPages[tabIndex] },
                        SelectionTypes.Primary

                    );
                }
            }
        }

        /// <summary>
        /// Highlights the selected button in the control by applying a bold font style and an underline.
        /// </summary>
        /// <remarks>This method iterates through all controls and applies a bold font and underline to
        /// the button that corresponds to the currently selected tab in the linked tab control. If no tab control is
        /// linked, the method performs no action.</remarks>
        public void HighlightSelected()
        {
            if (_linkedTabControl == null) return;

            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i] is ThemableButton btn)
                {
                    bool selected = i == _linkedTabControl.SelectedIndex;
                    btn.Font = selected ? new Font(Font, FontStyle.Bold) : new Font(Font, FontStyle.Regular);
                    btn.DrawUnderline = (i == _linkedTabControl.SelectedIndex);
                }
            }
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
        /// Applies the specified theme colors to the control and its child buttons.
        /// </summary>
        /// <remarks>This method updates the background, foreground, and border colors of the control
        /// based on the provided theme colors. If theming is disabled or the <paramref name="colors"/> parameter is
        /// null, the method will not apply any changes. The method also recursively applies the theme to any child
        /// buttons, updating their background and foreground colors.</remarks>
        /// <param name="colors">The <see cref="ThemeColors"/> object containing the colors to apply. Cannot be null.</param>
        public void ApplyTheme(ThemeColors colors)
        {

            if (!_useThemeColors || colors == null)
            {
                DebugUtils.LogIf(colors == null, "Theming", "ThemableTabControlHeader", "ThemeColors is null.");
                DebugUtils.LogIf(!_useThemeColors, "Theming", "ThemableTabControlHeader", "Theming is disabled.");
                return;
            }

            _themeColors = colors;
            BackColor = colors.BackColor;
            ForeColor = Enabled ? colors.ForeColor : colors.DisabledColor;
            if (MatchBorderColor)
                _underlineColor = Enabled ? colors.BorderColor : colors.DisabledColor;
            else
                _underlineColor = Enabled ? colors.AccentColor : colors.DisabledColor;
            Invalidate();

            //Recursively apply theme to buttons
            foreach (Control control in Controls)
            {
                if (control is Button btn)
                {
                    btn.BackColor = BackColor;
                    btn.ForeColor = ForeColor;
                    btn.Invalidate();
                }
            }
        }
        #endregion IThemableControl Implementation

    }
}
