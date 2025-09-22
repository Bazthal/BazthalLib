using System;
using System.Collections.Generic;
using System.Linq;

namespace BazthalLib.Events
{
    /// <summary>
    /// Provides data for an event that occurs when items are reordered.
    /// </summary>
    /// <remarks>This event argument contains the old and new indices of the reordered items, allowing
    /// subscribers  to determine how the order of items has changed.</remarks>
    public class ItemsReorderedEventArgs : EventArgs
    {
        public IReadOnlyList<int> OldIndices { get; }
        public IReadOnlyList<int> NewIndices { get; }

        /// <summary>
        /// Provides data for an event that occurs when items are reordered.
        /// </summary>
        /// <remarks>Both <paramref name="oldIndices"/> and <paramref name="newIndices"/> are expected to
        /// have the same number of elements,  with each index corresponding to the same item before and after the
        /// reorder.</remarks>
        /// <param name="oldIndices">The original indices of the items before the reorder operation.</param>
        /// <param name="newIndices">The new indices of the items after the reorder operation.</param>
        public ItemsReorderedEventArgs(IEnumerable<int> oldIndices, IEnumerable<int> newIndices)
        {
            OldIndices = oldIndices.ToList().AsReadOnly();
            NewIndices = newIndices.ToList().AsReadOnly();
        }
    }
}
