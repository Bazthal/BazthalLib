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


        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        /// <summary>
        /// Retrieves a collection of standard values representing the names of all <see cref="TabControl"/> components
        /// within the designer host container. This is used to provide a list of available <see cref="TabControl"/> names
        /// for selection in design-time property grids.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context. May be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="TypeConverter.StandardValuesCollection"/> containing the names of all <see cref="TabControl"/> components
        /// in the container, or an empty collection if no container is available.
        /// </returns>
        {
            if (context?.Container == null)
                return new StandardValuesCollection(Array.Empty<string>());

            // Enumerate all TabControl components in the designer host container
            var tabNames = context.Container.Components
                .OfType<TabControl>()
                .Where(tc => !string.IsNullOrEmpty(tc.Name))
                .Select(tc => tc.Name)
                .OrderBy(name => name)
                .ToList();

            return new StandardValuesCollection(tabNames);
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
