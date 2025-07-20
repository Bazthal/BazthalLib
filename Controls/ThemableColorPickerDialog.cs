using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BazthalLib.UI;

namespace BazthalLib.Controls
{
    public class ThemableColorPickerDialog : Form
    {

        #region Fields and Properties
        private Color _selectedColor = Color.FromArgb(255, 255, 255, 255); // Default to white with full alpha It's like this since the dialog outputs ARGB values.

        /// <summary>
        /// Represents a collection of preset colors available for selection.
        /// </summary>
        /// <remarks>These colors are defined using their common names for ease of use and are converted
        /// to ARGB values when used in a dialog. The collection includes a variety of basic and commonly used
        /// colors.</remarks>
        Color[] preset = new[]
        {
            Color.Black, Color.White, Color.Gray, Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple,
            Color.Brown, Color.Cyan, Color.Magenta, Color.Yellow, Color.Lime, Color.Pink, Color.Teal, Color.Navy
        };
        /// <summary>
        /// Represents an array of predefined colors.
        /// </summary>
        /// <remarks>This array includes a selection of common colors such as black, white, red, green,
        /// and blue,  as well as additional colors like orange, purple, and lavender. It can be used to provide  a
        /// standard set of colors for applications requiring color selection or display.</remarks>
        Color[] expandedPreset = new[]
{
            Color.Black, Color.White, Color.Gray, Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple,
            Color.Brown, Color.Cyan, Color.Magenta, Color.Yellow, Color.Lime, Color.Pink, Color.Teal, Color.Navy,
            Color.Ivory, Color.Lavender, Color.DodgerBlue, Color.Khaki, Color.LawnGreen, Color.HotPink, Color.MintCream, Color.Indigo
        };

        private bool _useThemeColors = true;
        private ThemeColors _themeColors;
        private bool _suppressInputUpdate = false;
        private bool _suppressSliderUpdate = false;
        private bool _isExpanded = false;

        private Point? colorWheelPip = null;
        private bool isDraggingWheel = false;

        private EyedropperOverlay overlay;
        public Color SelectedColor => _selectedColor;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog uses theme colors.
        /// </summary>
        [Browsable(true)]
        [Category("BazthalLib - Appearance")]
        [Description("Use theme colors for the dialog.")]
        [DefaultValue(true)]
        public bool UseThemeColors
        {
            get => _useThemeColors;
            set { _useThemeColors = value; Invalidate(); }
        }
        #endregion Fields and Properties


        #region controls

        private ThemablePanel previewPanel;
        private ThemableButton okButton, cancelButton;
        private ThemableButton expandButton;
        private ThemableButton[] swatchButtons;
        private ThemableNumericUpDown aBox, rBox, gBox, bBox;
        private ThemableTextBox hexBox;
        private ThemablePictureBox colorWheel;
        private ThemableTrackBar hueSlider, satSlider, lightSlider, contrastSlider;
        private Label hueLabel, satLabel, lightLabel;
        private ThemableButton eyedropperButton;

        #endregion controls



        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableColorPickerDialog"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the color picker dialog with default settings and prepares
        /// it for use. It initializes the form, sets up controls, arranges them, and hooks necessary events.</remarks>
        public ThemableColorPickerDialog()
        {
            InitializeForm();
            InitializeControls();
            LayoutControls();
            HookEvents();

            AddSwatches(preset);
            UpdateFromColor(_selectedColor);
        }
        #endregion Constructor

        #region Initialization Helpers
        /// <summary>
        /// Initializes the form with predefined settings for appearance and behavior.
        /// </summary>
        /// <remarks>This method sets the form's title, size, border style, and start position.  It also
        /// disables the maximize and minimize buttons. The form is registered  with the theming library to apply
        /// consistent UI styling.</remarks>
        private void InitializeForm()
        {
            BazthalLib.UI.Theming.RegisterForm(this);
            Text = "Choose Color";
            Size = new Size(300, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
        }
        /// <summary>
        /// Initializes and configures the UI controls for the color selection interface.
        /// </summary>
        /// <remarks>This method sets up various controls including color preview panels, RGBA input
        /// boxes, a hexadecimal color input box, and buttons for confirming or canceling the selection. It also
        /// initializes a color wheel and sliders for adjusting hue, saturation, and lightness.</remarks>
        private void InitializeControls()
        {

            previewPanel = new ThemablePanel { Size = new Size(60, 60), Location = new Point(20, 20), UseAccentColor = true, BackColor = _selectedColor };

            rBox = CreateChannelBox("R:", new Point(100, 20));
            gBox = CreateChannelBox("G:", new Point(100, 60));
            bBox = CreateChannelBox("B:", new Point(100, 100));
            aBox = CreateChannelBox("A:", new Point(100, 140));

            hexBox = new ThemableTextBox { Location = new Point(100, 180), Width = 80 };
            okButton = new ThemableButton { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(100, 290) };
            cancelButton = new ThemableButton { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(180, 290) };
            expandButton = new ThemableButton { Text = ">>", Location = new Point(230, 180), Size = new Size(40, 25) };

            colorWheel = new ThemablePictureBox { Size = new Size(200, 200), Location = new Point(350, 10), EnableBorder = false, };


            (hueSlider, hueLabel) = AddSlider("Hue:", new Point(300, 220), 0, 360);
            (satSlider, satLabel) = AddSlider("Saturation:", new Point(300, 260), 0, 100);
            (lightSlider, lightLabel) = AddSlider("Lightness:", new Point(300, 300), 0, 100);

            eyedropperButton = new ThemableButton
            {
                TintedImage = TintedImageRenderer.LoadEmbededImage("BazthalLib.Resources.ColorPicker.png"),
                UseAccentForTintedImage = true,
                FocusWrapAroundImage = true,
                Location = new Point(300, 180),
                EnableBorder = false
            };

        }

        /// <summary>
        /// Arranges the controls within the form, adding them to the control collection and setting their initial
        /// layout.
        /// </summary>
        /// <remarks>This method adds a predefined set of controls to the form's control collection and
        /// sets up an event handler to refresh certain sliders when the form is shown. It ensures that the controls are
        /// properly initialized and displayed when the form becomes visible.</remarks>
        private void LayoutControls()
        {


            Controls.AddRange(new Control[]
{
                previewPanel, rBox, gBox, bBox, aBox,
                new Label { Text = "#", Location = new Point(85, 183), Width = 15 },
                hexBox, okButton, cancelButton, expandButton, colorWheel,
                hueSlider, hueLabel, satSlider, satLabel,
                lightSlider, lightLabel, eyedropperButton
});

            this.Shown += (s, e) =>
            {
                foreach (var slider in new[] { hueSlider, satSlider, lightSlider })
                {
                    slider.Invalidate();
                    slider.Update();
                }
            };
        }

        /// <summary>
        /// Attaches event handlers to various UI components for handling user interactions and input changes.
        /// </summary>
        /// <remarks>This method sets up event listeners for input fields, sliders, and UI elements to
        /// ensure that changes in the user interface are captured and processed. It updates the internal state based on
        /// user input from ARGB fields, hex input, sliders, and other UI controls.</remarks>
        private void HookEvents()
        {
            //ARGB input events
            rBox.ValueChanged += (_, _) => { if (!_suppressInputUpdate) UpdateFromInputs(); };
            gBox.ValueChanged += (_, _) => { if (!_suppressInputUpdate) UpdateFromInputs(); };
            bBox.ValueChanged += (_, _) => { if (!_suppressInputUpdate) UpdateFromInputs(); };
            aBox.ValueChanged += (_, _) => { if (!_suppressInputUpdate) UpdateFromInputs(); };
            hexBox.TextChanged += (_, _) => { if (!_suppressInputUpdate) UpdateFromHex(); };

            //Slider events
            hueSlider.Scroll += (_, _) => UpdateFromSliders();
            satSlider.Scroll += (_, _) => UpdateFromSliders();
            lightSlider.Scroll += (_, _) => UpdateFromSliders();

            // UI events
            colorWheel.Paint += DrawColorWheel;
            //colorWheel.MouseClick += ColorWheel_MouseClick;
            colorWheel.MouseDown += ColorWheel_MouseDown;
            colorWheel.MouseMove += ColorWheel_MouseMove;
            colorWheel.MouseUp += ColorWheel_MouseUp;
            expandButton.Click += (_, _) => ToggleExpandedView();

            // eyedropperButton.Click += (s, e) => StartEyedropper();
            eyedropperButton.Click += (s, e) => StartEyedropper();


        }


        #endregion Initialization Helpers

        #region Update Logic

        /// <summary>
        /// Sets the selected color and updates the UI components to reflect the new color.
        /// </summary>
        /// <param name="color">The new color to be selected and displayed.</param>
        private void SetSelectedColor(Color color)
        {
            _selectedColor = color;
            previewPanel.BackColor = color;
            SetArgbInputsFromColor(color);
            UpdateColorWheelPipFromColor(color);
            UpdateSliderFromColor(color);
            UpdateHexBox(color);
        }

        /// <summary>
        /// Updates the selected color based on the current values of the input boxes.
        /// </summary>
        /// <remarks>This method retrieves the ARGB values from the input boxes and updates the selected
        /// color accordingly. Ensure that the input values are within the valid range for ARGB components (0 to
        /// 255).</remarks>
        private void UpdateFromInputs()
        {
            var color = Color.FromArgb((int)aBox.Value, (int)rBox.Value, (int)gBox.Value, (int)bBox.Value);
            SetSelectedColor(color);
        }

        /// <summary>
        /// Updates the selected color based on the hexadecimal color code entered in the hex input box.
        /// </summary>
        /// <remarks>The method processes a hexadecimal string from the input box, which can be either 6
        /// or 8 characters long. If the string is 8 characters, the first two characters are treated as the alpha
        /// component. The method then converts the hexadecimal values to ARGB components and updates the selected
        /// color.</remarks>
        private void UpdateFromHex()
        {
            DebugUtils.Log("Color Picker", $"Hex input:", "{hexBox.Text}");

            try
            {
                var hex = hexBox.Text.Trim();
                if (hex.Length == 8 || hex.Length == 6)
                {
                    int a = 255, r, g, b;
                    if (hex.Length == 8)
                    {
                        a = Convert.ToInt32(hex.Substring(0, 2), 16);
                        hex = hex.Substring(2);
                    }
                    r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    SetSelectedColor(Color.FromArgb(a, r, g, b));
                }
            }
            catch { }
        }
        /// <summary>
        /// Updates the current state based on the specified color.
        /// </summary>
        /// <param name="selectedColor">The color to update the state with. Cannot be null.</param>
        private void UpdateFromColor(Color selectedColor)
        {
            SetSelectedColor(selectedColor);
        }
        /// <summary>
        /// Updates the slider controls to reflect the specified color's hue, saturation, and brightness values.
        /// </summary>
        /// <remarks>This method converts the given color into its hue, saturation, and brightness (HSB)
        /// components and updates the corresponding sliders and labels. The slider update is temporarily suppressed to
        /// prevent triggering additional events during the update process.</remarks>
        /// <param name="color">The <see cref="Color"/> to be converted into hue, saturation, and brightness values for the sliders.</param>
        private void UpdateSliderFromColor(Color color)
        {
            ColorToHSB(color, out var h, out var s, out var b);
            _suppressSliderUpdate = true;
            UpdateSliderAndLabel(hueSlider, hueLabel, (int)h);
            UpdateSliderAndLabel(satSlider, satLabel, (int)(s * 100));
            UpdateSliderAndLabel(lightSlider, lightLabel, (int)(b * 100));
            _suppressSliderUpdate = false;

            RefreshSliders();
        }


        /// <summary>
        /// Updates the selected color based on the current values of the hue, saturation, and lightness sliders.
        /// </summary>
        /// <remarks>This method converts the HSV values from the sliders to an RGB color and sets it as
        /// the selected color. The update is suppressed if the <see cref="_suppressSliderUpdate"/> flag is set to <see
        /// langword="true"/>.</remarks>
        private void UpdateFromSliders()
        {
            if (_suppressSliderUpdate) return;

            double h = hueSlider.Value;
            double s = satSlider.Value / 100.0;
            double v = lightSlider.Value / 100.0;
            var rgb = HsvToRgb(h, s, v);
            var finalColor = Color.FromArgb((int)aBox.Value, rgb.R, rgb.G, rgb.B);
            SetSelectedColor(finalColor);
        }



        #endregion Update Logic

        #region Helper Methods

        /// <summary>
        /// Initiates the eyedropper tool, allowing the user to select a color from the screen.
        /// </summary>
        /// <remarks>This method displays an overlay and starts a mouse hook to detect when the user
        /// clicks on the screen. The eyedropper operation can be canceled by pressing the Escape key.</remarks>
        public void StartEyedropper()
        {
            overlay = new EyedropperOverlay();
            overlay.EscapePressed += CancelEyedropper;
            overlay.Show();

            MouseHook.Start(OnEyedropperClick);
        }

        /// <summary>
        /// Cancels the eyedropper operation and restores the default cursor.
        /// </summary>
        /// <remarks>This method stops tracking the mouse and closes any overlay associated with the
        /// eyedropper tool.</remarks>
        private void CancelEyedropper()
        {
            MouseHook.Stop(); // stop tracking mouse
            Cursor = Cursors.Default;
            overlay?.Close();
            overlay = null;
        }
        /// <summary>
        /// Handles the click event for the eyedropper tool, capturing the color at the specified screen position.
        /// </summary>
        /// <remarks>This method captures the color of a single pixel at the specified position on the
        /// screen and sets it as the selected color. It also closes any existing overlay and restores the default
        /// cursor.</remarks>
        /// <param name="position">The screen coordinates where the eyedropper was clicked.</param>
        private void OnEyedropperClick(Point position)
        {

            overlay?.Close();
            overlay = null;

            Cursor = Cursors.Default; // restore

            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(position, Point.Empty, new Size(1, 1));
            var color = bmp.GetPixel(0, 0);

            SetSelectedColor(color);

            // this.Show(); // Show the form again after using the eyedropper
        }
        /// <summary>
        /// Creates a numeric input control for a color channel with a specified label and location.
        /// </summary>
        /// <param name="label">The text label to display next to the numeric input control.</param>
        /// <param name="location">The location on the form where the label and numeric input control will be placed.</param>
        /// <returns>A <see cref="ThemableNumericUpDown"/> control configured for color channel input, with a range from 0 to
        /// 255.</returns>
        private ThemableNumericUpDown CreateChannelBox(string label, Point location)
        {
            var lbl = new Label { Text = label, Location = location, Width = 20 };
            var box = new ThemableNumericUpDown
            {
                Location = new Point(location.X + 25, location.Y),
                Maximum = 255,
                Minimum = 0,
                Width = 50
            };
            Controls.Add(lbl);
            return box;
        }

        /// <summary>
        /// Updates the background color of each swatch button to match its associated color tag.
        /// </summary>
        /// <remarks>This method iterates over all swatch buttons and sets their background color to the
        /// color specified in their tag. Ensure that each button's tag is of type <see cref="Color"/> to avoid
        /// unexpected behavior.</remarks>
        public void RefreshSwatchButtons()
        {

            foreach (var btn in swatchButtons)
            {
                if (btn.Tag is Color swatchColor)
                {
                    btn.BackColor = swatchColor;
                }

            }
        }
        /// <summary>
        /// Adds a slider control with an associated label to the form at the specified location.
        /// </summary>
        /// <remarks>The slider is configured with a fixed width and other visual properties, and its
        /// value is displayed in a label next to it. The label updates automatically when the slider is
        /// moved.</remarks>
        /// <param name="name">The text to display on the label associated with the slider.</param>
        /// <param name="location">The <see cref="Point"/> specifying the location of the label on the form.</param>
        /// <param name="min">The minimum value of the slider.</param>
        /// <param name="max">The maximum value of the slider.</param>
        /// <returns>A tuple containing the created <see cref="ThemableTrackBar"/> slider and its associated value <see
        /// cref="Label"/>.</returns>
        private (ThemableTrackBar slider, Label valueLabel) AddSlider(string name, Point location, int min, int max)
        {
            var lbl = new Label { Text = name, Location = location, AutoSize = true };
            Controls.Add(lbl);

            var slider = new ThemableTrackBar
            {
                Minimum = min,
                Maximum = max,
                EnableBorder = false,
                TrackMargin = 10,
                ThumbSize = 12,
                UseProgressFill = false,
                Width = 160,
                Location = new Point(location.X + 60, location.Y - 2)
            };
            Controls.Add(slider);

            var valLabel = new Label
            {
                Text = slider.Value.ToString(),
                Location = new Point(slider.Right + 5, slider.Top + 3),
                Width = 40
            };
            Controls.Add(valLabel);

            slider.Scroll += (_, _) =>
            {
                valLabel.Text = slider.Value.ToString();
                Slider_Scroll(slider, EventArgs.Empty); // trigger color update
            };

            return (slider, valLabel);
        }
        /// <summary>
        /// Updates the specified slider and label with the given value.
        /// </summary>
        /// <param name="slider">The slider control to update. The slider's value will be set to the specified value, clamped within its
        /// minimum and maximum range.</param>
        /// <param name="label">The label control to update. The label's text will be set to the string representation of the specified
        /// value.</param>
        /// <param name="value">The value to set for the slider and label. The slider's value will be clamped to its valid range.</param>
        private void UpdateSliderAndLabel(ThemableTrackBar slider, Label label, int value)
        {
            if (slider.Value != value)
                slider.Value = Math.Clamp(value, slider.Minimum, slider.Maximum);
            label.Text = value.ToString();

        }

        /// <summary>
        /// Handles the scroll event for the slider controls, updating the selected color based on the current slider
        /// values.
        /// </summary>
        /// <remarks>This method updates the selected color by converting the current hue, saturation, and
        /// lightness values from the sliders into an RGB color. It then updates the color wheel and ARGB input fields
        /// to reflect the new color. The method prevents recursive updates by checking the <see
        /// cref="_suppressSliderUpdate"/> flag.</remarks>
        /// <param name="sender">The source of the event, typically a slider control.</param>
        /// <param name="e">The event data associated with the scroll event.</param>
        private void Slider_Scroll(object sender, EventArgs e)
        {
            if (_suppressSliderUpdate) return; // prevent recursive updates

            double hue = hueSlider.Value;
            double sat = satSlider.Value / 100.0;
            double val = lightSlider.Value / 100.0;

            Color c = HsvToRgb(hue, sat, val);
            //c = ApplyContrast(c, contrast);

            UpdateColorWheelPipFromColor(c);

            SetArgbInputsFromColor(c);
            _selectedColor = c;
            UpdateFromInputs();


        }
        /// <summary>
        /// Refreshes the expanded view of the control, ensuring that the display is updated.
        /// </summary>
        /// <remarks>This method invalidates the current view, forces an immediate update, and refreshes
        /// any associated sliders. It should be called whenever the expanded view needs to be redrawn or updated due to
        /// changes in the control's state.</remarks>
        private void RefreshExpandedView()
        {
            this.Invalidate();
            this.Update();
            RefreshSliders();
        }

        /// <summary>
        /// Refreshes the display of the hue, saturation, and lightness sliders and their associated labels.
        /// </summary>
        /// <remarks>This method invalidates and updates each slider and label to ensure the UI reflects
        /// the current state. It is typically called when the underlying data changes and the UI needs to be
        /// refreshed.</remarks>
        private void RefreshSliders()
        {
            foreach (var slider in new[] { hueSlider, satSlider, lightSlider })
            {
                slider.Invalidate();
                slider.Update();


                foreach (var label in new[] { hueLabel, satLabel, lightLabel })
                {
                    label.Invalidate();
                    label.Update();
                }
            }

        }
        /// <summary>
        /// Updates the ARGB input controls to reflect the specified color values.
        /// </summary>
        /// <remarks>This method sets the values of the ARGB input controls to match the corresponding
        /// components of the provided color. The input values are clamped to the valid range of each control. The
        /// controls are invalidated and updated to ensure the display reflects the new values.</remarks>
        /// <param name="color">The <see cref="Color"/> object whose ARGB values are used to update the input controls.</param>
        private void SetArgbInputsFromColor(Color color)
        {
            _suppressInputUpdate = true;
            try
            {
                rBox.Value = Clamp(color.R, rBox.Minimum, rBox.Maximum);
                gBox.Value = Clamp(color.G, gBox.Minimum, gBox.Maximum);
                bBox.Value = Clamp(color.B, bBox.Minimum, bBox.Maximum);
                aBox.Value = Clamp(color.A, aBox.Minimum, aBox.Maximum);

                foreach (var box in new[] { rBox, gBox, bBox, aBox })
                {
                    box.Invalidate();
                    box.Update();
                }
            }
            finally
            {
                _suppressInputUpdate = false;
            }
        }

        /// <summary>
        /// Updates the text of the hexBox control to reflect the specified color in ARGB hexadecimal format.
        /// </summary>
        /// <remarks>The method temporarily suppresses input updates to prevent recursive changes while
        /// setting the hexBox text.</remarks>
        /// <param name="color">The <see cref="Color"/> to be converted to a hexadecimal string and displayed in the hexBox.</param>
        private void UpdateHexBox(Color color)
        {
            _suppressInputUpdate = true;
            hexBox.Text = $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            _suppressInputUpdate = false;
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to its equivalent hue, saturation, and brightness (HSB) values.
        /// </summary>
        /// <remarks>The method calculates the HSB values based on the RGB components of the input color.
        /// The hue is calculated in degrees, while saturation and brightness are represented as fractions of
        /// 1.</remarks>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <param name="hue">When this method returns, contains the hue component of the color, measured in degrees from 0 to 360.</param>
        /// <param name="saturation">When this method returns, contains the saturation component of the color, ranging from 0 to 1.</param>
        /// <param name="value">When this method returns, contains the brightness component of the color, ranging from 0 to 1.</param>
        private void ColorToHSB(Color color, out double hue, out double saturation, out double value)
        {
            System.Diagnostics.Debug.WriteLine($"ColorToHSB: {color}"); // Debug output
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            value = max / 255.0;

            saturation = max == 0 ? 0 : 1.0 - (1.0 * min / max);

            hue = 0;
            if (max == min)
                hue = 0;
            else if (max == color.R)
                hue = 60 * ((color.G - color.B) / (double)(max - min)) % 360;
            else if (max == color.G)
                hue = 60 * ((color.B - color.R) / (double)(max - min)) + 120;
            else if (max == color.B)
                hue = 60 * ((color.R - color.G) / (double)(max - min)) + 240;

            if (hue < 0)
                hue += 360;
        }

        /// <summary>
        /// Updates the position of the color wheel pip based on the specified color.
        /// </summary>
        /// <remarks>The method converts the provided color to its hue, saturation, and brightness (HSB)
        /// values, and calculates the pip's position on the color wheel accordingly. The color wheel is then
        /// invalidated and updated to reflect the new pip position.</remarks>
        /// <param name="color">The color used to determine the new position of the color wheel pip.</param>
        private void UpdateColorWheelPipFromColor(Color color)
        {
            ColorToHSB(color, out double hue, out double saturation, out double lightness);
            int radius = colorWheel.Width / 2;
            Point center = new Point(radius, radius);
            double angle = hue * (Math.PI / 180.0);
            double distance = saturation * radius;

            int x = (int)(center.X + Math.Cos(angle) * distance);
            int y = (int)(center.Y + Math.Sin(angle) * distance);


            colorWheelPip = new Point(x, y);
            colorWheel.Invalidate(); // force redraw
            colorWheel.Update();

        }
        /// <summary>
        /// Renders a color wheel onto the associated control, displaying a spectrum of colors in a circular format.
        /// </summary>
        /// <remarks>The method creates a bitmap representing a color wheel, where each pixel's color is
        /// determined by its angle and distance from the center. The color wheel is drawn with a smooth transition from
        /// fully opaque at the center to transparent at the edges. If a pip position is specified, it is drawn as a
        /// small circle on the wheel. In debug mode, the generated color wheel image is saved to the application's
        /// startup path.</remarks>
        /// <param name="sender">The source of the paint event, typically the control that is being painted.</param>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data, including the graphics context used for
        /// drawing.</param>
        private void DrawColorWheel(object sender, PaintEventArgs e)
        {
            int width = colorWheel.Width;
            int height = colorWheel.Height;
            Bitmap bmp = new Bitmap(width, height);
            int radius = width / 2;
            Point center = new Point(radius, radius);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = x - center.X;
                    int dy = y - center.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= radius)
                    {
                        double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
                        if (angle < 0) angle += 360;
                        double saturation = distance / radius;

                        Color rgb = HsvToRgb(angle, saturation, 1.0);

                        // Feather the edge: fully opaque in middle, fades out at edge
                        double fadeThreshold = 0.97;
                        double alpha = 1.0;
                        double normalized = distance / radius;

                        if (normalized > fadeThreshold)
                        {
                            alpha = 1.0 - (normalized - fadeThreshold) / (1.0 - fadeThreshold);
                            alpha = Math.Clamp(alpha, 0.0, 1.0);
                        }

                        Color finalColor = Color.FromArgb((int)(alpha * 255), rgb);
                        bmp.SetPixel(x, y, finalColor);
                    }
                    else
                    {
                        bmp.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
            if (DebugUtils.DebugMode)
            {
                Image img = bmp;
                img.Save(Application.StartupPath + "ColorWheel.png", ImageFormat.Png);
            }

            // Draw pip if available
            if (colorWheelPip is Point p)
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.Black, 2))
                    {
                        g.DrawEllipse(pen, p.X - 3, p.Y - 3, 6, 6);
                    }
                }
            }

            // Draw with smoothing onto control
            colorWheel.Image = bmp;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(bmp, Point.Empty);
        }

        /// <summary>
        /// Handles the mouse down event on the color wheel to update the selected color.
        /// </summary>
        /// <remarks>If the middle mouse button is pressed, the selected color is reset to white and the
        /// UI is updated accordingly. If the left mouse button is pressed, it initiates a drag operation to select a
        /// color from the wheel.</remarks>
        /// <param name="sender">The source of the event, typically the color wheel control.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data.</param>
        private void ColorWheel_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Middle)
            {
                //_selectedColor = Color.White;
                SetArgbInputsFromColor(_selectedColor);
                UpdateColorWheelPipFromColor(_selectedColor);
                UpdateSliderFromColor(_selectedColor);
                UpdateHexBox(_selectedColor);
                previewPanel.BackColor = _selectedColor;
                colorWheel.Invalidate();
                return;


            }
            if (e.Button != MouseButtons.Left) return; // only handle left button drag

            isDraggingWheel = true;
            HandleColorWheelInteraction(e.Location);
            colorWheel.Invalidate(); // force redraw
        }

        /// <summary>
        /// Handles the mouse move event for the color wheel, allowing color selection through dragging.
        /// </summary>
        /// <remarks>This method processes mouse movements when the left mouse button is held down,
        /// enabling interaction with the color wheel. It updates the color selection based on the current mouse
        /// position and triggers a redraw of the color wheel.</remarks>
        /// <param name="sender">The source of the event, typically the color wheel control.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> containing the event data, including the mouse button state and location.</param>
        private void ColorWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return; // only handle left button drag

            if (isDraggingWheel)
            {
                HandleColorWheelInteraction(e.Location);
                colorWheel.Invalidate(); // force redraw
            }
        }

        /// <summary>
        /// Handles the MouseUp event for the color wheel, ending any active drag operation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ColorWheel_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingWheel = false;
        }

        /// <summary>
        /// Toggles the view between expanded and collapsed states.
        /// </summary>
        /// <remarks>This method adjusts the size and layout of the form, updates button positions, and
        /// refreshes the swatch buttons to reflect the current view state.</remarks>
        private void ToggleExpandedView()
        {
            // Implement smooth transition and make sure it's not too abrupt
            //Right now it just changes the text and size of the form
            if (_isExpanded)
            {
                expandButton.Text = ">>";
                Size = new Size(300, 360);

                okButton.Location = new Point(100, 290);
                cancelButton.Location = new Point(180, 290);

                AddSwatches(preset);
                RefreshSwatchButtons();
                _isExpanded = false;
            }
            else
            {
                expandButton.Text = "<<";
                Size = new Size(600, 380);

                okButton.Location = new Point(100, 310);
                cancelButton.Location = new Point(180, 310);

                AddSwatches(expandedPreset);
                RefreshSwatchButtons();
                _isExpanded = true;
            }
            RefreshExpandedView();
        }
        /// <summary>
        /// Adds a set of color swatches to the control, allowing users to select from the provided colors.
        /// </summary>
        /// <remarks>Each color in the array is represented by a button, which is added to the control.
        /// Clicking a button updates the selected color and related UI elements.</remarks>
        /// <param name="color">An array of <see cref="Color"/> objects representing the colors to be used for the swatches. Cannot be null
        /// or empty.</param>
        private void AddSwatches(Color[] color)
        {

            if (swatchButtons != null)
            {
                foreach (var btn in swatchButtons)
                {
                    Controls.Remove(btn);
                }
            }
            swatchButtons = new ThemableButton[color.Length];

            int swatchesPerRow = 8;
            int startX = 20;
            int startY = 220;
            int spacing = 30;

            //for (int i = 0; i < presets.Length; i++)
            for (int i = 0; i < color.Length; i++)
            {
                int row = i / swatchesPerRow;
                int col = i % swatchesPerRow;

                var btn = new ThemableButton
                {
                    Size = new Size(25, 25),
                    Location = new Point(startX + col * spacing, startY + row * spacing),
                    BackColor = color[i],
                    FlatStyle = FlatStyle.Flat,
                    Tag = color[i],
                    AllowFocus = false, // prevent focus on click
                    AllowClickHighlight = false //prevent highlight on click this is to prevent the swatch color changing when clicked                 
                };

                btn.Click += (_, _) =>
                {
                    var chosenColor = (Color)btn.Tag;
                    SetArgbInputsFromColor(chosenColor);
                    _selectedColor = chosenColor;

                    ColorToHSB(chosenColor, out double hue, out double saturation, out double lightness);
                    UpdateSliderFromColor(chosenColor);
                    UpdateColorWheelPipFromColor(chosenColor);
                    UpdateFromInputs(); // ensure hex box is updated
                };

                swatchButtons[i] = btn;
                Controls.Add(btn);

            }
        }

        /// <summary>
        /// Restricts a given integer value to be within a specified range.
        /// </summary>
        /// <param name="value">The integer value to be clamped.</param>
        /// <param name="min">The minimum allowable value. The result will not be less than this value.</param>
        /// <param name="max">The maximum allowable value. The result will not exceed this value.</param>
        /// <returns>The clamped value, which will be within the range defined by <paramref name="min"/> and <paramref
        /// name="max"/>.</returns>
        private decimal Clamp(int value, decimal min, decimal max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        /// <summary>
        /// Converts an HSV color value to an RGB color.
        /// </summary>
        /// <remarks>The method calculates the RGB representation of a color based on the provided hue,
        /// saturation, and value. The resulting RGB values are scaled to the range [0, 255].</remarks>
        /// <param name="h">The hue of the color, in degrees. Must be in the range [0, 360).</param>
        /// <param name="s">The saturation of the color, as a value between 0 and 1.</param>
        /// <param name="v">The value (brightness) of the color, as a value between 0 and 1.</param>
        /// <returns>A <see cref="Color"/> structure representing the equivalent RGB color.</returns>
        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = (int)(h / 60) % 6;
            double f = h / 60 - Math.Floor(h / 60);

            v *= 255;
            int vi = (int)v;
            int p = (int)(v * (1 - s));
            int q = (int)(v * (1 - f * s));
            int t = (int)(v * (1 - (1 - f) * s));

            return hi switch
            {
                0 => Color.FromArgb(vi, t, p),
                1 => Color.FromArgb(q, vi, p),
                2 => Color.FromArgb(p, vi, t),
                3 => Color.FromArgb(p, q, vi),
                4 => Color.FromArgb(t, p, vi),
                _ => Color.FromArgb(vi, p, q)
            };
        }

        /// <summary>
        /// Handles user interaction with the color wheel by selecting a color based on the specified location.
        /// </summary>
        /// <remarks>This method updates the selected color and related UI elements based on the user's
        /// interaction with the color wheel. It ignores interactions outside the color wheel's radius or with
        /// transparent or nearly black colors.</remarks>
        /// <param name="location">The point on the color wheel where the interaction occurred.</param>
        private void HandleColorWheelInteraction(Point location)
        {
            if (colorWheel.Image is not Bitmap bmp) return;

            int radius = bmp.Width / 2;
            Point center = new Point(radius, radius);
            int dx = location.X - radius;
            int dy = location.Y - radius;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance > radius) return;

            if (location.X < 0 || location.Y < 0 || location.X >= bmp.Width || location.Y >= bmp.Height)
                return;

            var color = bmp.GetPixel(location.X, location.Y);

            _selectedColor = color;
            colorWheelPip = location;

            if (color.A == 0 || (color.R < 5 && color.G < 5 && color.B < 5)) return; // ignore transparent or black colors

            // Convert to HSB and update sliders
            ColorToHSB(color, out double h, out double s, out double b);
            UpdateSliderFromColor(color);

            previewPanel.BackColor = color;
            colorWheel.Invalidate();
            SetArgbInputsFromColor(color);
            UpdateFromInputs();

            previewPanel.BackColor = color;

            colorWheel.Invalidate(); // redraw pip
        }

        /// <summary>
        /// Retrieves the color of the pixel located at the current cursor position on the screen.
        /// </summary>
        /// <remarks>This method captures a single pixel from the screen at the cursor's location and
        /// returns its color. It is useful for applications that need to sample screen colors dynamically.</remarks>
        /// <returns>A <see cref="Color"/> structure representing the color of the pixel at the cursor's current position.</returns>
        public Color GetColorAtCursor()
        {
            Bitmap bmp = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Cursor.Position, Point.Empty, new Size(1, 1));
            }
            return bmp.GetPixel(0, 0);
        }


        #endregion Helper Methods

        #region Dialog
        /// <summary>
        /// Displays a color picker dialog and returns the selected color if confirmed.
        /// </summary>
        /// <remarks>The dialog allows users to pick a color and confirms the selection upon acceptance. 
        /// If the dialog is canceled, the method returns <see langword="null"/>.</remarks>
        /// <returns>A <see cref="Color"/> representing the selected color if the dialog result is <see cref="DialogResult.OK"/>;
        /// otherwise, <see langword="null"/>.</returns>
        public static new Color? Show()
        {
            using var dlg = new ThemableColorPickerDialog();
            dlg.RefreshSwatchButtons(); // Makes sure the swatches have the correct colors


            return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedColor : null;
        }
        #endregion Dialog
    }
}