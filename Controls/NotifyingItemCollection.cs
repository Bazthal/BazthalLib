using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BazthalLib.Controls
{
    public class NotifyingItemCollection : IList
    {
        private readonly ListBox.ObjectCollection _baseCollection;
        private readonly Control _owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyingItemCollection"/> class,  which wraps a <see
        /// cref="ListBox.ObjectCollection"/> and provides notification capabilities.
        /// </summary>
        /// <remarks>This collection is designed to provide additional functionality, such as
        /// notifications,  on top of the standard <see cref="ListBox.ObjectCollection"/>. It is intended to be used  in
        /// scenarios where changes to the collection need to be tracked or communicated to other components.</remarks>
        /// <param name="baseCollection">The underlying <see cref="ListBox.ObjectCollection"/> that this collection wraps.</param>
        /// <param name="owner">The <see cref="Control"/> that owns this collection, typically used for notification purposes.</param>
        public NotifyingItemCollection(ListBox.ObjectCollection baseCollection, Control owner)
        {
            _baseCollection = baseCollection;
            _owner = owner;
        }

        /// <summary>
        /// Adds an object to the collection and invalidates the owner.
        /// </summary>
        /// <remarks>After adding the object, the owner is invalidated, which may trigger a refresh or
        /// update of the owner's state.</remarks>
        /// <param name="value">The object to add to the collection. This value cannot be null.</param>
        /// <returns>The zero-based index at which the object has been added.</returns>
        public int Add(object value)
        {
            int index = _baseCollection.Add(value);
            _owner.Invalidate();
            return index;
        }

        /// <summary>
        /// Adds a range of elements to the collection.
        /// </summary>
        /// <remarks>After adding the elements, the collection owner is invalidated to reflect the
        /// changes.</remarks>
        /// <param name="values">The collection of elements to add. Cannot be null.</param>
        public void AddRange(IEnumerable<object> values)
        {
            foreach (var value in values)
            {
                _baseCollection.Add(value);
            }
            _owner.Invalidate();
        }
        /// <summary>
        /// Removes the specified object from the collection.
        /// </summary>
        /// <remarks>After the object is removed, the owner is invalidated to reflect the
        /// change.</remarks>
        /// <param name="value">The object to remove from the collection. Must not be <see langword="null"/>.</param>
        public void Remove(object value)
        {
            _baseCollection.Remove(value);
            _owner.Invalidate();
        }
        /// <summary>
        /// Clears all elements from the collection.
        /// </summary>
        /// <remarks>This method removes all items from the underlying collection and invalidates the
        /// owner, which may trigger a refresh or update in the associated UI or data structure.</remarks>
        public void Clear()
        {
            _baseCollection.Clear();
            _owner.Invalidate();
        }
        /// <summary>
        /// Gets or sets the element at the specified index in the collection.
        /// </summary>
        /// <remarks>Setting the element at the specified index will invalidate the owner, triggering any
        /// necessary updates.</remarks>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns></returns>
        public object this[int index]
        {
            get => _baseCollection[index];
            set
            {
                _baseCollection[index] = value;
                _owner.Invalidate();
            }
        }
        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _baseCollection.Count;
        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => ((IList)_baseCollection).IsReadOnly;
        /// <summary>
        /// Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        public bool IsFixedSize => ((IList)_baseCollection).IsFixedSize;
        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public object SyncRoot => ((IList)_baseCollection).SyncRoot;
        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => ((IList)_baseCollection).IsSynchronized;
        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the collection. The value can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the specified value is found in the collection; otherwise, <see
        /// langword="false"/>.</returns>
        public bool Contains(object value)
        {
            //Return false if value us null
            if (value == null) return false;
            return _baseCollection.Contains(value);
        }
        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the
        /// collection.
        /// </summary>
        /// <param name="value">The object to locate in the collection. The value can be <see langword="null"/>.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="value"/> within the collection, if found;
        /// otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            //Return -1 if value is null
            if (value == null) return -1;
            return _baseCollection.IndexOf(value);
        }
        /// <summary>
        /// Inserts an object into the collection at the specified index.
        /// </summary>
        /// <remarks>After the insertion, the collection will contain the new object at the specified
        /// index, and all subsequent elements will be shifted one position.</remarks>
        /// <param name="index">The zero-based index at which the object should be inserted.</param>
        /// <param name="value">The object to insert into the collection. This value can be <see langword="null"/>.</param>
        public void Insert(int index, object value)
        {
            _baseCollection.Insert(index, value);
            _owner.Invalidate();
        }
        /// <summary>
        /// Removes the element at the specified index from the collection.
        /// </summary>
        /// <remarks>After removal, the elements that follow the removed element move up to occupy the
        /// vacated spot. The collection is invalidated after the removal operation.</remarks>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (index == 0 && _baseCollection.Count == 0) return;
            _baseCollection.RemoveAt(index);
            _owner.Invalidate();
        }
        /// <summary>
        /// Copies the elements of the collection to a specified array, starting at a particular index.
        /// </summary>
        /// <remarks>If the destination array is of type <see cref="object[]"/>, the elements are copied
        /// directly. Otherwise, each element is individually boxed or unboxed as necessary.</remarks>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must
        /// have zero-based indexing.</param>
        /// <param name="index">The zero-based index in the array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            if (array is object[] objArray)
            {
                _baseCollection.CopyTo(objArray, index);
            }
            else
            {
                // Fallback: manually copy with boxing/unboxing safety
                for (int i = 0; i < _baseCollection.Count; i++)
                {
                    array.SetValue(_baseCollection[i], i + index);
                }
            }
        }

        public IEnumerator GetEnumerator() => _baseCollection.GetEnumerator();
    }


}
