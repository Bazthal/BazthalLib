using System;
using System.Drawing;
using System.Windows.Forms;

namespace BazthalLib
{
    public class EyedropperOverlay : Form
    {

        #region Fields and Properties
        public event Action EscapePressed;
        private Label instructionLabel;
        protected override bool ShowWithoutActivation => true;



        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EyedropperOverlay"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the overlay form with specific properties to facilitate its
        /// use as an eyedropper tool. The form is borderless, starts at a manual position, and is not shown in the
        /// taskbar. It is set to be always on top and uses a cross cursor to indicate its function. The background
        /// color is black with a semi-transparent opacity. The form's bounds are set to cover the entire virtual
        /// screen.</remarks>
        public EyedropperOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Cursor = Cursors.Cross;
            BackColor = Color.Black;
            Opacity = 0.4;
            Bounds = GetVirtualScreenBounds();

            //Load += (_, _) => MakeClickThrough();
            Load += (_, _) => ShowInstructions();
            KeyPreview = true;
            KeyDown += OnKeyDown;
        }
        #endregion Constructor


        #region Methods and Events
        /// <summary>
        /// Handles the KeyDown event for the associated control.
        /// </summary>
        /// <remarks>This method checks if the Escape key was pressed and, if so, invokes the <see
        /// cref="EscapePressed"/> event.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EscapePressed?.Invoke();
            }
        }

        private Rectangle GetVirtualScreenBounds() => new Rectangle(
            SystemInformation.VirtualScreen.Left,
            SystemInformation.VirtualScreen.Top,
            SystemInformation.VirtualScreen.Width,
            SystemInformation.VirtualScreen.Height
            );

        /// <summary>
        /// Displays an instruction label on the overlay, guiding the user on how to interact with the application.
        /// </summary>
        /// <remarks>The label provides instructions for picking a color by clicking anywhere on the
        /// screen and for canceling the operation using the Esc key. The label is centered on the overlay and styled
        /// with a bold font for visibility.</remarks>
        private void ShowInstructions()
        {
            instructionLabel = new Label
            {
                Text = "🖱 Click anywhere to pick a color | ⎋ Esc to cancel",
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(instructionLabel);

            // Center label on the overlay
            instructionLabel.Location = new Point(
                (Width - instructionLabel.Width) / 2,
                (Height - instructionLabel.Height) / 2
            );
            DebugUtils.Log("Misc", $"{this.Name}", "Showing instruction label");
            //DebugUtils.Log("Misc", "EyeDropperOverlay", "Showing instruction label");
        }

        /// <summary>
        /// Gets the parameters required to create the window.
        /// </summary>
        /// <remarks>This property overrides the base implementation to modify the extended window style,
        /// ensuring that the window does not appear in the taskbar.</remarks>
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW; // avoid taskbar icon
                return cp;
            }
        }

        #endregion Methods and Events
    }

}
