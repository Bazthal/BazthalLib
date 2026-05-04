using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BazthalLib.DebugUtils;

namespace BazthalLib.UI
{
    /// <summary>
    /// Specifies the modes for reusing forms in an application.
    /// </summary>
    /// <remarks>This enumeration defines the strategies for handling form instances when new forms are
    /// created.</remarks>
    public enum FormReuseMode
    {
        None,
        Rotate,
        AutoCloseOldest
    }

    /// <summary>
    /// Represents information about a form instance, including its usage statistics and label.
    /// </summary>
    /// <remarks>This class provides details about when the form was opened, last used, and how many times it
    /// has been reused. It also allows for an optional label to be associated with the form instance.</remarks>
    internal sealed class FormInstanceInfo
    {
        public Form Form { get; }
        public DateTime OpenedAt { get; }
        public DateTime LastUsed { get; private set; }
        public int ReuseCount { get; private set; }
        public string? Label { get; set; }

        /// <summary>
        /// Represents information about a form instance, including its associated form, label, and usage statistics.
        /// </summary>
        /// <remarks>The instance records the time it was opened and tracks the last time it was used, as
        /// well as the number of times it has been reused.</remarks>
        /// <param name="form">The form associated with this instance. Cannot be null.</param>
        /// <param name="label">An optional label for the form instance. Can be null.</param>
        public FormInstanceInfo(Form form, string? label = null)
        {
            Form = form;
            Label = label;
            OpenedAt = DateTime.Now;
            LastUsed = OpenedAt;
            ReuseCount = 0;
        }

        /// <summary>
        /// Marks the current instance as used by updating the last used timestamp and incrementing the reuse count.
        /// </summary>
        /// <remarks>This method updates the <see cref="LastUsed"/> property to the current date and time,
        /// and increments the <see cref="ReuseCount"/> property. It is typically called to track usage statistics of
        /// the instance.</remarks>
        public void MarkUsed()
        {
            LastUsed = DateTime.Now;
            ReuseCount++;
        }

        /// <summary>
        /// Returns a string representation of the form's state, including its type, open time, reuse count, and label.
        /// </summary>
        /// <returns>A string that contains the form's type name, the time it was opened, the number of times it has been reused,
        /// and its label. If the label is null, "N/A" is used instead.</returns>
        public override string ToString()
            => $"{Form.GetType().Name} (Opened {OpenedAt:HH:mm:ss}, Reused {ReuseCount}x, Label='{Label ?? "N/A"}')";
    }

    public class WinForms
    {
        /// <summary>
        /// A collection that maps a type to a list of form instance information.
        /// </summary>
        /// <remarks>This dictionary is used to store and retrieve form instances associated with specific
        /// types. It is initialized as an empty dictionary and is intended for internal use only.</remarks>
        private static readonly Dictionary<Type, List<FormInstanceInfo>> _formInstances = new();

        /// <summary>
        /// Opens a new instance of the specified form type, or reuses an existing instance according to the provided
        /// options. Supports single or multiple instances, custom initialization, and configurable reuse behavior.
        /// </summary>
        /// <remarks>If an existing form instance is reused, it is brought to the front and restored to
        /// its normal window state. When multiple instances are allowed and the maximum is reached, the behavior is
        /// determined by <paramref name="reuseMode"/>. The <paramref name="action"/> delegate is invoked after the form
        /// is opened or reused. The method automatically tracks and removes disposed or closed forms from its internal
        /// list. Thread safety is not guaranteed; calls should be made from the UI thread.</remarks>
        /// <typeparam name="TForm">The type of form to open. Must inherit from <see cref="Form"/>.</typeparam>
        /// <param name="action">An optional action to perform on the form after it is opened or reused. Can be used to initialize or modify
        /// the form before it is displayed.</param>
        /// <param name="centerToParent">Indicates whether the form should be centered relative to its parent form. If <see langword="true"/>, the
        /// form is centered to its parent; otherwise, default positioning is used.</param>
        /// <param name="rectenterOnFocus">Indicates whether the form should be re-centered when it receives focus. If <see langword="true"/>, the form
        /// is re-centered on focus.</param>
        /// <param name="allowMultiple">Indicates whether multiple instances of the form are allowed. If <see langword="true"/>, multiple instances
        /// can be opened; otherwise, only a single instance is permitted.</param>
        /// <param name="maxInstances">The maximum number of form instances allowed when <paramref name="allowMultiple"/> is <see
        /// langword="true"/>. If set to 0, there is no limit.</param>
        /// <param name="reuseMode">Specifies the strategy for reusing or rotating form instances when the maximum number of instances is
        /// reached. Determines whether to close, rotate, or reuse existing forms.</param>
        /// <param name="args">Optional constructor arguments to pass when creating a new instance of the form.</param>
        /// <returns>The opened or reused instance of the specified form type.</returns>
        public static TForm OpenForm<TForm>(Action<TForm>? action = null, bool centerToParent = true, bool rectenterOnFocus = false, bool allowMultiple = false, int maxInstances = 0, FormReuseMode reuseMode = FormReuseMode.None, params object[]? args)
            where TForm : Form
        {
            var formType = typeof(TForm);
            if (!_formInstances.TryGetValue(formType, out var list))
            {
                list = new List<FormInstanceInfo>();
                _formInstances[formType] = list;
            }

            list.RemoveAll(info => info.Form.IsDisposed || !info.Form.Visible);

            if (!allowMultiple)
            {
                var existing = list.FirstOrDefault()?.Form;
                if (existing != null)
                {
                    existing.BringToFront();
                    existing.WindowState = FormWindowState.Normal;
                    action?.Invoke((TForm)existing);
                    DebugUtils.Log("WinForms", $"OpenForm<{formType.Name}>", "Reused single-instance form.", logLevel: LogLevel.Info);
                    return (TForm)existing;
                }
            }
            else if (maxInstances > 0 && list.Count >= maxInstances)
            {
                switch (reuseMode)
                {
                    case FormReuseMode.AutoCloseOldest:
                        {
                            var oldest = list.FirstOrDefault();
                            if (oldest != null)
                            {
                                oldest.Form.Close();
                                list.Remove(oldest);
                                DebugUtils.Log("WinForms", $"OpenForm<{formType.Name}>", "Auto-closed oldest instance.", logLevel: LogLevel.Info);
                            }
                            break;
                        }

                    case FormReuseMode.Rotate:
                        {
                            var oldest = list.FirstOrDefault();
                            if (oldest is { Form: TForm rotated })
                            {
                                oldest.MarkUsed();
                                action?.Invoke(rotated);
                                rotated.WindowState = FormWindowState.Normal;
                                rotated.BringToFront();

                                list.RemoveAt(0);
                                list.Add(oldest);

                                DebugUtils.Log("WinForms", $"OpenForm<{formType.Name}>",
                                    $"Rotated oldest → newest | {oldest}", logLevel: LogLevel.Info);
                                return rotated;
                            }
                            break;
                        }

                    case FormReuseMode.None:
                    default:
                        {
                            var newest = list.LastOrDefault();
                            if (newest is { Form: TForm reused })
                            {
                                newest.MarkUsed();
                                action?.Invoke(reused);
                                reused.WindowState = FormWindowState.Normal;
                                reused.BringToFront();

                                DebugUtils.Log("WinForms", $"OpenForm<{formType.Name}>",
                                    $"Reused newest form (limit reached) | {newest}", logLevel: LogLevel.Info);
                                return reused;
                            }
                            break;
                        }
                }
            }

            TForm newForm = args is { Length: > 0 }
                ? (TForm)Activator.CreateInstance(formType, args)!
                : (TForm)Activator.CreateInstance(formType)!;

            if (centerToParent)
            {
                var parent = Form.ActiveForm;
                if (parent != null)
                {
                    newForm.StartPosition = FormStartPosition.Manual;
                    newForm.Location = new Point(
                        parent.Left + (parent.Width - newForm.Width) / 2,
                        parent.Top + (parent.Height - newForm.Height) / 2);
                }
                else
                {
                    newForm.StartPosition = FormStartPosition.CenterScreen;
                }
            }
            var info = new FormInstanceInfo(newForm);

            newForm.FormClosed += (_, _) =>
            {
                if (_formInstances.TryGetValue(formType, out var tracked))
                    tracked.RemoveAll(i => i.Form == newForm);
            };

            list.Add(info);
            newForm.Show();
            action?.Invoke(newForm);

            DebugUtils.Log("WinForms", $"OpenForm<{formType.Name}>",
                $"Opened new instance ({list.Count}/{maxInstances}) | {info}", logLevel: LogLevel.Info);

            return newForm;
        }

        /// <summary>
        /// Opens a form of the specified type. If a form of that type is already open, it brings it to the front and
        /// restores it if minimized.
        /// </summary>
        /// <remarks>If a form of the specified type is already open, it will be brought to the front and
        /// restored to its normal window state if it is minimized. If no such form is open, a new instance of the form
        /// will be created and shown.</remarks>
        /// <param name="formType">The <see cref="Type"/> of the form to open. Must be a subclass of <see cref="Form"/>.</param>
        /// <param name="action">An optional <see cref="Action{Form}"/> to execute on the form after it is opened or brought to the front.
        /// Can be <see langword="null"/>.</param>
        [Obsolete("Use OpenForm<TForm>() instead. The generic version supports constructor arguments, returns the opened form, and provides better type safety.", false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void OpenForm(Type formType, Action<Form>? action = null)
        {
            DebugUtils.LogObsoleteUsage("WinForms.OpenForm(Type)", "WinForms.OpenForm<TForm>()");
            var openForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.GetType() == formType);

            if (openForm != null)
            {
                openForm.BringToFront();
                openForm.WindowState = FormWindowState.Normal; // Restore if minimized
                action?.Invoke(openForm); // Run the function if provided
            }
            else
            {
                Form newForm = (Form)Activator.CreateInstance(formType);
                //                DarkNet.Instance.SetWindowThemeForms(newForm, Theme.Auto);
                newForm.Show();

                action?.Invoke(newForm); // Run the function on the new form
            }
        }

        /// <summary>
        /// Closes one or more instances of a specified form type.
        /// </summary>
        /// <remarks>This method first attempts to close forms that are being tracked internally. If no
        /// tracked instances are found, it falls back to closing the first open form of the specified type from <see
        /// cref="Application.OpenForms"/>. Forms that are already disposed or not visible are automatically excluded
        /// from the tracked list.</remarks>
        /// <typeparam name="TForm">The type of the form to close. Must derive from <see cref="Form"/>.</typeparam>
        /// <param name="closeAll">If <see langword="true"/>, closes all tracked instances of the specified form type. If <see
        /// langword="false"/>, closes only the newest or oldest instance based on the value of <paramref
        /// name="closeOldest"/>.</param>
        /// <param name="closeOldest">If <see langword="true"/>, closes the oldest tracked instance of the specified form type. If <see
        /// langword="false"/>, closes the newest tracked instance. Ignored if <paramref name="closeAll"/> is <see
        /// langword="true"/>.</param>
        public static void CloseForm<TForm>(bool closeAll = false, bool closeOldest = false)
            where TForm : Form
        {
            var formType = typeof(TForm);

            if (_formInstances.TryGetValue(formType, out var list) && list.Count > 0)
            {
                list.RemoveAll(info => info.Form.IsDisposed || !info.Form.Visible);

                if (list.Count == 0)
                    return;

                if (closeAll)
                {
                    foreach (var info in list.ToList())
                        info.Form.Close();

                    list.Clear();
                    DebugUtils.Log("WinForms", $"CloseForm<{formType.Name}>",
                        "Closed all tracked instances.", logLevel: LogLevel.Info);
                }
                else if (closeOldest)
                {
                    var oldest = list.FirstOrDefault();
                    if (oldest != null)
                    {
                        oldest.Form.Close();
                        list.Remove(oldest);
                        DebugUtils.Log("WinForms", $"CloseForm<{formType.Name}>",
                            $"Closed oldest tracked instance | {oldest}", logLevel: LogLevel.Info);
                    }
                }
                else
                {
                    var newest = list.LastOrDefault();
                    if (newest != null)
                    {
                        newest.Form.Close();
                        list.Remove(newest);
                        DebugUtils.Log("WinForms", $"CloseForm<{formType.Name}>",
                            $"Closed newest tracked instance | {newest}", logLevel: LogLevel.Info);
                    }
                }
            }
            else
            {
                var openForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.GetType() == formType);
                openForm?.Close();
                DebugUtils.Log("WinForms", $"CloseForm<{formType.Name}>",
                    "Closed via Application.OpenForms fallback (untracked form).", logLevel: LogLevel.Info);
            }
        }

        /// <summary>
        /// Closes the first open form of the specified type.
        /// </summary>
        /// <remarks>If no open form of the specified type is found, the method does nothing.</remarks>
        /// <param name="formType">The <see cref="Type"/> of the form to close. Must not be <see langword="null"/>.</param>
        [Obsolete("Use CloseForm<TForm>() instead. The generic version provides better type safety.", false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void CloseForm(Type formType)
        {
            DebugUtils.LogObsoleteUsage("WinForms.CloseForm(Type)", "WinForms.CloseForm<TForm>()");
            var openForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.GetType() == formType);
            openForm?.Close();
        }
    }
}
