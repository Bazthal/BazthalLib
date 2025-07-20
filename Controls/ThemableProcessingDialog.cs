using System;
using System.Drawing;
using System.Windows.Forms;

namespace BazthalLib.Controls
{
    public class ThemableProcessingDialog : Form
    {
        #region Fields
        private readonly ThemableLabel statusLabel;
        private readonly ThemableProgressBar progressBar;
        private string _lastProgressText = "";
        private string? _lastEtaText = null;
        private readonly string _version = "V1.1";
        #endregion Fields

        #region Constuctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemableProcessingDialog"/> class with a specified title and
        /// progress display option.
        /// </summary>
        /// <remarks>This constructor sets up a dialog with a fixed size and position, disables the
        /// maximize and minimize buttons,  and centers the dialog relative to its parent. The dialog includes a status
        /// label and a progress bar,  both of which are themable.</remarks>
        /// <param name="title">The title of the dialog window. Defaults to "Processing".</param>
        /// <param name="showProgress">A boolean value indicating whether the progress bar should display progress continuously or as a marquee. 
        /// <see langword="true"/> for continuous progress; otherwise, <see langword="false"/> for marquee style.</param>
        public ThemableProcessingDialog(string title = "Processing", bool showProgress = true)
        {
            BazthalLib.UI.Theming.RegisterForm(this);
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(400, 80);
            this.ControlBox = false;

            statusLabel = new ThemableLabel
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 10),
                Size = new Size(380, 20),
                Text = "Processing..."
            };

            progressBar = new ThemableProgressBar
            {
                Location = new Point(10, 35),
                Size = new Size(380, 20),
                Style = showProgress ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee,
                Visible = true,
                EnableBorder = true,
                //UseAccentColor = true
            };

            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
        }
        #endregion Constuctor

        #region Methods and Events

        /// <summary>
        /// Updates the progress display with the current status of a processing task.
        /// </summary>
        /// <remarks>This method updates a status label and a progress bar to reflect the current progress
        /// of a task. If the method is called from a thread other than the UI thread, it will invoke itself on the UI
        /// thread.</remarks>
        /// <param name="type">The type of the task being processed, used for display purposes.</param>
        /// <param name="current">The current progress count, indicating how much of the task has been completed.</param>
        /// <param name="total">The total count representing the full extent of the task.</param>
        /// <param name="eta">The estimated time of arrival (ETA) for task completion. If null or empty, the last known ETA is used.</param>
        /// <param name="forceUiUpdate">If <see langword="true"/>, forces an immediate update of the UI to reflect the current progress.</param>
        public void SetProgress(string type, int current, int total, string? eta = null, bool forceUiUpdate = false)
        {
            if (InvokeRequired)
            {
                this.Invoke(() => SetProgress(type, current, total, eta, forceUiUpdate));
                return;
            }

            // Cache and reuse last valid ETA
            if (!string.IsNullOrEmpty(eta))
                _lastEtaText = eta;

            string etaText = _lastEtaText ?? "ETA: --:--";
            string newText = $"Processing {type} {current} of {total}...  |  {etaText}";

            if (newText != _lastProgressText)
            {
                statusLabel.Text = newText;
                _lastProgressText = newText;
            }

            if (progressBar.Style != ProgressBarStyle.Marquee)
            {
                progressBar.Maximum = total;
                progressBar.Value = current;
            }

             if (forceUiUpdate)
            Application.DoEvents();
        }

        /// <summary>
        /// Sets the progress bar to marquee style and updates the status label with the specified message.
        /// </summary>
        /// <remarks>If called from a thread other than the UI thread, the method will invoke itself on
        /// the UI thread.</remarks>
        /// <param name="message">The message to display in the status label. Defaults to "Processing...".</param>
        public void SetMarquee(string message = "Processing...")
        {
            if (InvokeRequired)
            {
                this.Invoke(() => SetMarquee(message));
                return;
            }

            progressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = message;
            Application.DoEvents();
        }

        /// <summary>
        /// Marks the progress as completed and updates the status message.
        /// </summary>
        /// <remarks>If called from a thread other than the UI thread, this method will invoke itself on
        /// the UI thread.</remarks>
        /// <param name="message">The message to display in the status label upon completion.</param>
        public void SetCompleted(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(() => SetCompleted(message));
                return;
            }

           // progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = progressBar.Maximum;
            statusLabel.Text = message;
            Application.DoEvents();
        }

        /// <summary>
        /// Closes the form after a specified delay.
        /// </summary>
        /// <remarks>This method uses a timer to close the form after the specified delay. The form will
        /// remain open for the duration of the delay.</remarks>
        /// <param name="milliseconds">The delay in milliseconds before the form is closed. Must be a non-negative integer.</param>
        public void CloseAfter(int milliseconds)
        {
            Timer timer = new Timer { Interval = milliseconds };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                this.Close();
            };
            timer.Start();
        }
        #endregion Methods and Events

    }
}
