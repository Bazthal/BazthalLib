using System.Drawing;
using System.Windows.Forms;
using BazthalLib.Controls;
using BazthalLib.UI;

namespace BazthalLib
{
    public class ThemableMessageBox : Form
    {
        #region Fields and Properties
        private DialogResult _result = DialogResult.None;

        private readonly int _autoCloseMilliseconds;

        #endregion Fields and Properties

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableMessageBox"/> class with the specified message, title,
        /// buttons, icon, and optional auto-close duration.
        /// </summary>
        /// <remarks>The message box is centered relative to its parent and has a fixed tool window border
        /// style. It does not show an icon in the taskbar and cannot be maximized or minimized. If <paramref
        /// name="autoCloseMilliseconds"/> is greater than 0, the message box will automatically close after the
        /// specified duration.</remarks>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box window.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box. Defaults to <see cref="MessageBoxIcon.None"/>.</param>
        /// <param name="autoCloseMilliseconds">The time in milliseconds after which the message box will automatically close. If set to 0, the message box
        /// will not auto-close.</param>
        public ThemableMessageBox(
            string message,
            string title,
            MessageBoxButtons buttons,
            MessageBoxIcon icon = MessageBoxIcon.None,
            int autoCloseMilliseconds = 0)
        {
            _autoCloseMilliseconds = autoCloseMilliseconds;

            Text = title;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(300, 180);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;

            var layout = CreateLayout(message, buttons, icon);
            Controls.Add(layout);

            Theming.RegisterForm(this);

            if (_autoCloseMilliseconds > 0)
                StartAutoCloseTimer();
        }
        #endregion Constructor

        #region Methods and Events
        /// <summary>
        /// Starts a timer that automatically closes the dialog after a specified interval.
        /// </summary>
        /// <remarks>The timer is configured to trigger after the interval defined by
        /// <c>_autoCloseMilliseconds</c>. When the timer elapses, the dialog is closed and the result is set to <see
        /// cref="DialogResult.None"/>.</remarks>
        private void StartAutoCloseTimer()
        {
            var timer = new Timer
            {
                Interval = _autoCloseMilliseconds
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _result = DialogResult.None;
                Close();
            };
            timer.Start();
        }
        #endregion Methods and Events

        #region Layout and Controls
        /// <summary>
        /// Creates a layout for a message box with specified message, buttons, and icon.
        /// </summary>
        /// <remarks>The layout includes a message label and an optional icon, arranged in a <see
        /// cref="TableLayoutPanel"/>. The buttons are created in a separate panel and added to the main
        /// layout.</remarks>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="buttons">The buttons to include in the message box, specified by the <see cref="MessageBoxButtons"/> enumeration.</param>
        /// <param name="icon">The icon to display in the message box, specified by the <see cref="MessageBoxIcon"/> enumeration. If <see
        /// cref="MessageBoxIcon.None"/>, no icon is displayed.</param>
        /// <returns>A <see cref="Control"/> containing the constructed layout for the message box.</returns>
        private Control CreateLayout(string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            var hasIcon = icon != MessageBoxIcon.None;

            var iconBox = new PictureBox
            {
                Size = new Size(48, 48),
                Margin = new Padding(20),
                Image = GetIconImage(icon),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = hasIcon // Hide the icon if none
            };

            var label = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size(370, 0),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var contentPanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = hasIcon ? 2 : 1,
                RowCount = 1,
                Padding = new Padding(10),
                Dock = DockStyle.Top
            };

            if (hasIcon)
            {
                contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                contentPanel.Controls.Add(iconBox, 0, 0);
                contentPanel.Controls.Add(label, 1, 0);
            }
            else
            {
                contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                contentPanel.Controls.Add(label, 0, 0);
            }

            var buttonPanel = CreateButtonPanel(buttons);

            var mainPanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            mainPanel.Controls.Add(contentPanel, 0, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);

            // Dynamically adjust the form size only after layout is built
            Load += (_, _) =>
            {
                ClientSize = mainPanel.PreferredSize + new Size(20, 20); // Padding for border
            };

            return mainPanel;
        }

        /// <summary>
        /// Creates a <see cref="FlowLayoutPanel"/> containing buttons based on the specified <see
        /// cref="MessageBoxButtons"/> configuration.
        /// </summary>
        /// <remarks>The panel's buttons are configured with appropriate <see cref="DialogResult"/> values
        /// and default button settings. The method also sets the <see cref="Form.AcceptButton"/> and <see
        /// cref="Form.CancelButton"/> properties based on the button configuration.</remarks>
        /// <param name="buttons">The <see cref="MessageBoxButtons"/> enumeration value that determines the set of buttons to display.</param>
        /// <returns>A <see cref="FlowLayoutPanel"/> with buttons configured to match the specified <paramref name="buttons"/>
        /// option.</returns>
        private FlowLayoutPanel CreateButtonPanel(MessageBoxButtons buttons)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 10),
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                WrapContents = false
            };

            void AddBtn(string text, DialogResult result, bool isDefault = false)
            {
                var btn = new ThemableButton
                {
                    Text = text,
                    DialogResult = result,
                    AutoSize = true,
                    Margin = new Padding(6, 0, 0, 0),
                    FlatStyle = FlatStyle.Flat
                };

                btn.Click += (_, _) =>
                {
                    _result = result;
                    Close();
                };

                if (isDefault)
                    AcceptButton = btn;

                panel.Controls.Add(btn);
            }

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    AddBtn("OK", DialogResult.OK, true);
                    break;
                case MessageBoxButtons.OKCancel:
                    AddBtn("OK", DialogResult.OK, true);
                    AddBtn("Cancel", DialogResult.Cancel);
                    CancelButton = (IButtonControl)panel.Controls[1];
                    break;
                case MessageBoxButtons.YesNo:
                    AddBtn("Yes", DialogResult.Yes, true);
                    AddBtn("No", DialogResult.No);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AddBtn("Yes", DialogResult.Yes, true);
                    AddBtn("No", DialogResult.No);
                    AddBtn("Cancel", DialogResult.Cancel);
                    CancelButton = (IButtonControl)panel.Controls[2];
                    break;
                case MessageBoxButtons.RetryCancel:
                    AddBtn("Retry", DialogResult.Retry, true);
                    AddBtn("Cancel", DialogResult.Cancel);
                    CancelButton = (IButtonControl)panel.Controls[1];
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    AddBtn("Abort", DialogResult.Abort, true);
                    AddBtn("Retry", DialogResult.Retry);
                    AddBtn("Ignore", DialogResult.Ignore);
                    break;
                case MessageBoxButtons.CancelTryContinue:
                    AddBtn("Cancel", DialogResult.Ignore);
                    AddBtn("Try", DialogResult.TryAgain);
                    AddBtn("Continue", DialogResult.Continue, true);
                    CancelButton = (IButtonControl)panel.Controls[0];
                    break;
            }

            return panel;
        }
        /// <summary>
        /// Retrieves the icon image corresponding to the specified <see cref="MessageBoxIcon"/>.
        /// </summary>
        /// <param name="icon">The <see cref="MessageBoxIcon"/> for which to retrieve the icon image.</param>
        /// <returns>An <see cref="Image"/> representing the specified icon. Returns <see langword="null"/> if the icon is not
        /// recognized.</returns>
        private Image GetIconImage(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => SystemIcons.Information.ToBitmap(),
                MessageBoxIcon.Warning => SystemIcons.Warning.ToBitmap(),
                MessageBoxIcon.Error => SystemIcons.Error.ToBitmap(),
                MessageBoxIcon.Question => SystemIcons.Question.ToBitmap(),
                _ => null
            };
        }
        /// <summary>
        /// Creates a custom layout for a dialog box with a specified message, custom buttons, and an icon.
        /// </summary>
        /// <param name="message">The message to display in the dialog box.</param>
        /// <param name="customButtons">An array of tuples, each containing the button text and the corresponding <see cref="DialogResult"/>.</param>
        /// <param name="icon">The <see cref="MessageBoxIcon"/> to display in the dialog box.</param>
        /// <returns>A <see cref="Control"/> containing the custom layout with the specified message, buttons, and icon.</returns>
        private Control CreateCustomLayout(string message, (string, DialogResult)[] customButtons, MessageBoxIcon icon)
        {
            var iconBox = new PictureBox
            {
                Size = new Size(48, 48),
                Margin = new Padding(20),
                Image = GetIconImage(icon),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var label = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size(600, 0),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10)
            };

            var contentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentPanel.Controls.Add(iconBox, 0, 0);
            contentPanel.Controls.Add(label, 1, 0);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 10),
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                WrapContents = false
            };

            foreach (var (text, result) in customButtons)
            {
                var btn = new ThemableButton
                {
                    Text = text,
                    DialogResult = result,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(6, 0, 0, 0)
                };
                btn.Click += (_, _) =>
                {
                    _result = result;
                    Close();
                };
                buttonPanel.Controls.Add(btn);
            }


            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.Controls.Add(contentPanel, 0, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);
            return mainPanel;
        }

        #endregion Layout and Controls

        #region DialogResult Methods

/// <summary>
/// Displays a message box with the specified message and an OK button.
/// </summary>
/// <param name="message">The message to display in the message box.</param>
/// <returns>A <see cref="DialogResult"/> value indicating the result of the message box interaction. Always returns <see
/// cref="DialogResult.OK"/> since only an OK button is provided.</returns>
        public static DialogResult Show(
            string message
            )
        {
            using var box = new ThemableMessageBox(message, "", MessageBoxButtons.OK, MessageBoxIcon.None);
            return box.ShowDialog();
        }

/// <summary>
/// Displays a message box with the specified message and title.
/// </summary>
/// <param name="message">The message to be displayed in the message box.</param>
/// <param name="title">The title of the message box window.</param>
/// <returns>A <see cref="DialogResult"/> value indicating the result of the message box interaction.</returns>
        public static DialogResult Show(
            string message,
            string title
            )
        {
            using var box = new ThemableMessageBox(message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
            return box.ShowDialog();
        }

/// <summary>
/// Displays a message box with the specified text, title, and buttons.
/// </summary>
/// <param name="message">The text to display in the message box.</param>
/// <param name="title">The title of the message box.</param>
/// <param name="buttons">A value that specifies which buttons to display in the message box.</param>
/// <returns>A <see cref="DialogResult"/> value that indicates which button was clicked by the user.</returns>
        public static DialogResult Show(
            string message,
            string title,
            MessageBoxButtons buttons
            )
        {
            using var box = new ThemableMessageBox(message, title, buttons, MessageBoxIcon.None);
            return box.ShowDialog();
        }

/// <summary>
/// Displays a message box with the specified text, title, buttons, and icon.
/// </summary>
/// <param name="message">The text to display in the message box.</param>
/// <param name="title">The title of the message box.</param>
/// <param name="buttons">The buttons to display in the message box. The default is <see cref="MessageBoxButtons.OK"/>.</param>
/// <param name="icon">The icon to display in the message box. The default is <see cref="MessageBoxIcon.None"/>.</param>
/// <returns>A <see cref="DialogResult"/> value that indicates which message box button was clicked by the user.</returns>
        public static DialogResult Show(
            string message,
            string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var box = new ThemableMessageBox(message, title, buttons, icon);
            return box.ShowDialog();
        }

        /// <summary>
        /// Displays a message box with a custom set of buttons and an icon.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box window.</param>
        /// <param name="customButtons">An array of tuples, each containing the text for a button and the <see cref="DialogResult"/> that should be
        /// returned when the button is clicked.</param>
        /// <param name="icon">The icon to display in the message box. Defaults to <see cref="MessageBoxIcon.None"/>.</param>
        /// <returns>A <see cref="DialogResult"/> value that indicates which button was clicked by the user.</returns>
        public static DialogResult Show(
            string message,
            string title,
            (string text, DialogResult result)[] customButtons,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var box = new ThemableMessageBox(message, title, MessageBoxButtons.OK, icon);
            box.Controls.Clear();
            box.Controls.Add(box.CreateCustomLayout(message, customButtons, icon));
            return box.ShowDialog();
        }

        /// <summary>
        /// Displays a message box with the specified text, title, buttons, and icon.
        /// </summary>
        /// <param name="message">The text to display in the message box.</param>
        /// <param name="title">The title of the message box window.</param>
        /// <param name="buttons">The buttons to display in the message box. The default is <see cref="MessageBoxButtons.OK"/>.</param>
        /// <param name="autoCloseMilliseconds">The time in milliseconds after which the message box will automatically close. A value of 0 means the
        /// message box will not close automatically.</param>
        /// <param name="icon">The icon to display in the message box. The default is <see cref="MessageBoxIcon.None"/>.</param>
        /// <returns>A <see cref="DialogResult"/> value that indicates which button was clicked by the user.</returns>
        public static DialogResult Show(
            string message,
            string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            int autoCloseMilliseconds = 0,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var box = new ThemableMessageBox(message, title, buttons, icon, autoCloseMilliseconds);
            return box.ShowDialog();
        }

        #endregion DialogResult Methods

    }
}
