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
        private string _version = "V1.2";
        private Color _underlineColor = SystemColors.ActiveBorder;
        private TabControl _linkedTabControl;
        private string _linkedTabControlName = "";
        private bool _useThemeColors = true;
        private ThemeColors _themeColors = new();

        //Button properties
        private bool _roundedButtonCorners = false;
        private int  _corenerRadius = 5;
        private bool _showButtonBorder = false;
        private bool _matchBorderColor = false;



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
        [Category("BazthalLib - Appearance")]
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
                    GenerateButtons();
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
                    GenerateButtons();
                    Invalidate();
                }
            }
        }


        
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Whether or not to round the coners for the border around the buttons.")]
        [DefaultValue(false)]
        public bool RoundedButtonCorners
        {
            get => _roundedButtonCorners;
            set
            {
                if (_roundedButtonCorners != value)
                {
                    _roundedButtonCorners = value;
                    GenerateButtons();
                    Invalidate();
                }
            }
        }


        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Radius to round the coners for the border around the buttons.")]
        [DefaultValue(false)]
        public int ButtonCornerRadius
        {
            get => _corenerRadius;
            set
            {
                if (_corenerRadius != value)
                {
                    _corenerRadius = value;
                    GenerateButtons();
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
        /// Attempts to resolve and link a <see cref="TabControl"/> to this header control.
        /// </summary>
        /// <remarks>
        /// This method first tries to find a <see cref="TabControl"/> by the explicit name specified in <see cref="LinkedTabControlName"/>,
        /// searching both the parent form and the design-time container. If no explicit name is set or found, it will automatically link
        /// to the first available <see cref="TabControl"/> in the design-time container or within the parent form's controls.
        /// Updates <see cref="LinkedTabControl"/> and <see cref="LinkedTabControlName"/> accordingly.
        /// </remarks>
        private void TryResolveTabControl()
        {
            var parent = FindForm();

            // First check by explicit name (runtime + design-time)
            if (!string.IsNullOrEmpty(_linkedTabControlName))
            {
                if (parent != null)
                {
                    var named = parent.Controls.Find(_linkedTabControlName, true).FirstOrDefault() as TabControl;
                    if (named != null)
                    {
                        LinkedTabControl = named;
                        return;
                    }
                }

                // Design-time: check container components
                if (Site?.Container != null)
                {
                    var named = Site.Container.Components
                        .OfType<TabControl>()
                        .FirstOrDefault(tc => tc.Name == _linkedTabControlName);
                    if (named != null)
                    {
                        LinkedTabControl = named;
                        return;
                    }
                }
            }

            // If no explicit name, auto-link to first TabControl
            if (IsInDesignMode() && Site?.Container != null)
            {
                var firstTabControl = Site.Container.Components
                    .OfType<TabControl>()
                    .FirstOrDefault();

                if (firstTabControl != null)
                {
                    _linkedTabControlName = firstTabControl.Name;
                    LinkedTabControl = firstTabControl;
                    return;
                }
            }
            else if (parent != null)
            {
                var firstTab = parent.Controls.OfType<Control>()
                    .SelectMany(c => GetAllControlsRecursive(c))
                    .OfType<TabControl>()
                    .FirstOrDefault();

                if (firstTab != null)
                {
                    _linkedTabControlName = firstTab.Name;
                    LinkedTabControl = firstTab;
                }
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
        /// Generates the tab header buttons for each <see cref="TabPage"/> in the linked <see cref="TabControl"/>.
        /// </summary>
        /// <remarks>
        /// This method clears any existing buttons and creates a new <see cref="ThemableButton"/> for each tab page.
        /// It attaches event handlers to update the button text when the tab page text changes and to handle button clicks.
        /// The method also highlights the selected tab and refreshes the control's appearance.
        /// </remarks>
        private void GenerateButtons()
        {
            if (_linkedTabControl == null) return;

            // Unhook previous handlers from TabPages
            foreach (TabPage tp in _linkedTabControl.TabPages)
                tp.TextChanged -= TabPage_TextChanged;

            Controls.Clear();

            for (int i = 0; i < _linkedTabControl.TabPages.Count; i++)
            {
                var tabPage = _linkedTabControl.TabPages[i];
                tabPage.TextChanged += TabPage_TextChanged; // Watch for header changes

                var btn = new ThemableButton
                {
                    Text = tabPage.Text.Replace("&", "&&"), // Escape ampersands
                    Tag = i,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(4),
                    AutoSize = true,
                    EnableBorder = _showButtonBorder,
                    AllowFocus = false,
                    AllowClickHighlight = false,
                    UseAccentForUnderline = !_matchBorderColor,
                    RoundCorners = _roundedButtonCorners,
                    CornerRadius = _corenerRadius,
                };

                btn.FlatAppearance.BorderSize = 0;
                btn.Click += TabButton_Click;

                Controls.Add(btn);
            }

            HighlightSelected();
            Invalidate();
            Refresh();
        }


        /// <summary>
        /// Handles the <see cref="TabPage.TextChanged"/> event for tab pages in the linked <see cref="TabControl"/>.
        /// </summary>
        /// <remarks>
        /// When the text of a tab page changes, this method regenerates the tab header buttons to reflect the updated text.
        /// The regeneration occurs only when the control is in design mode.
        /// </remarks>
        /// <param name="sender">The source of the event, typically a <see cref="TabPage"/> whose text has changed.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void TabPage_TextChanged(object sender, EventArgs e)
        {
            // Regenerate buttons when tab text changes
            if (IsInDesignMode())
                GenerateButtons();
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
