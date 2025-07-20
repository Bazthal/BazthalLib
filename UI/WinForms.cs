using System;
using System.Linq;
using System.Windows.Forms;

namespace BazthalLib.UI
{
    public class WinForms
    {
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
        public static void OpenForm(Type formType, Action<Form>? action = null)
        {
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
        /// Closes the first open form of the specified type.
        /// </summary>
        /// <remarks>If no open form of the specified type is found, the method does nothing.</remarks>
        /// <param name="formType">The <see cref="Type"/> of the form to close. Must not be <see langword="null"/>.</param>
        public static void CloseForm(Type formType)
        {
            var openForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.GetType() == formType);
            openForm?.Close();
        }
    }
}
