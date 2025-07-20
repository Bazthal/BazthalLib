using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace BazthalLib.Controls
{
    /// <summary>
    /// Provides a type converter to retrieve the names of all <see cref="TabControl"/> instances within the same form
    /// as the associated control.
    /// </summary>
    /// <remarks>This converter is useful for design-time support, allowing a property grid to display a list
    /// of available <see cref="TabControl"/> names for selection. It supports both standard values and manual
    /// entry.</remarks>
    public class TabControlNameConverter : StringConverter
    {
        /// <summary>
        /// Determines whether this object supports a standard set of values that can be picked from a list.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context. This parameter can be <see
        /// langword="null"/>.</param>
        /// <returns><see langword="true"/> if <see cref="GetStandardValues"/> should be called to find a common set of values
        /// the object supports; otherwise, <see langword="false"/>.</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            => true;

        /// <summary>
        /// Determines whether the list of standard values returned from <see cref="GetStandardValues"/> is an exclusive
        /// list.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context. This parameter can be <see
        /// langword="null"/>.</param>
        /// <returns><see langword="false"/> to indicate that the list of standard values is not exclusive, allowing users to
        /// enter values not in the list.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            => false; // Allow user to also type manually if needed

        /// <summary>
        /// Returns a collection of standard values representing the names of all <see cref="TabControl"/> instances
        /// within the form containing the specified control.
        /// </summary>
        /// <param name="context">The context in which the type descriptor is invoked. This parameter provides additional information about
        /// the environment from which this method is called.</param>
        /// <returns>A <see cref="StandardValuesCollection"/> containing the names of all <see cref="TabControl"/> instances
        /// within the form. Returns an empty collection if no <see cref="TabControl"/> instances are found or if the
        /// context is not associated with a <see cref="Control"/>.</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is Control control)
            {
                var form = control.FindForm();
                if (form != null)
                {
                    var tabControls = form.Controls.OfType<Control>()
                        .SelectMany(c => GetAllControlsRecursive(c))
                        .OfType<TabControl>()
                        .Select(t => t.Name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();

                    return new StandardValuesCollection(tabControls);
                }
            }

            return new StandardValuesCollection(Array.Empty<string>());
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
                       .SelectMany(c => new[] { c }.Concat(GetAllControlsRecursive(c)))
                       .ToArray();
        }
    }
}
