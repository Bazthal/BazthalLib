using System;
using System.Collections;
using System.Collections.Generic;

namespace BazthalLib.Systems
{
    public class LimitedStack<T> : IEnumerable<T>
    {
        private readonly object _lock = new object();
        private LinkedList<T> _list = new LinkedList<T>();

        public int Capacity { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitedStack{T}"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of elements the stack can hold.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
        public LimitedStack(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            Capacity = capacity;
        }

        /// <summary>
        /// Pushes an item onto the top of the stack. If the stack is at capacity, the oldest item is removed.
        /// </summary>
        /// <param name="item">The item to push onto the stack.</param>
        public void Push(T item)
        {
            lock (_lock)
            {
                if (_list.Count == Capacity)
                    _list.RemoveLast(); // drop oldest

                _list.AddFirst(item); // newest on top
            }
        }

        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The item removed from the top of the stack.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public T Pop()
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                    throw new InvalidOperationException("Stack is empty.");

                var value = _list.First.Value;
                _list.RemoveFirst();
                return value;
            }
        }

        /// <summary>
        /// Attempts to remove and return the item at the top of the stack.
        /// </summary>
        /// <param name="result">The item removed from the top of the stack, or the default value if the stack is empty.</param>
        /// <returns><c>true</c> if an item was removed; otherwise, <c>false</c>.</returns>
        public bool TryPop(out T result)
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    result = default!;
                    return false;
                }
                result = _list.First.Value;
                _list.RemoveFirst();
                return true;
            }
        }

        /// <summary>
        /// Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns>The item at the top of the stack.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public T Peek()
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                    throw new InvalidOperationException("Stack is empty.");

                return _list.First.Value;
            }
        }

        /// <summary>
        /// Attempts to return the item at the top of the stack without removing it.
        /// </summary>
        /// <param name="result">The item at the top of the stack, or the default value if the stack is empty.</param>
        /// <returns><c>true</c> if the stack is not empty; otherwise, <c>false</c>.</returns>
        public bool TryPeek(out T result)
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    result = default!;
                    return false;
                }
                result = _list.First.Value;
                return true;
            }
        }

        /// <summary>
        /// Removes all items from the stack.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _list.Clear();
            }
        }

        /// <summary>
        /// Determines whether the stack contains a specific item.
        /// </summary>
        /// <param name="item">The item to locate in the stack.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
        public bool Contains(T item)
        {
            lock (_lock)
            {
                return _list.Contains(item);
            }
        }

        /// <summary>
        /// Copies the stack to a new array.
        /// </summary>
        /// <returns>An array containing the elements of the stack.</returns>
        public T[] ToArray()
        {
            lock (_lock)
            {
                var array = new T[_list.Count];
                _list.CopyTo(array, 0);
                return array;
            }
        }

        /// <summary>
        /// Removes items from the stack if the number of items exceeds the current capacity.
        /// </summary>
        public void TrimExcess()
        {
            lock (_lock)
            {
                // In a LinkedList<T>, there's no "excess" memory to trim like a List<T>,
                // but we can drop extra items if capacity was reduced.
                while (_list.Count > Capacity)
                    _list.RemoveLast();
            }
        }

        /// <summary>
        /// Sets a new capacity for the stack and removes items if necessary.
        /// </summary>
        /// <param name="newCapacity">The new capacity for the stack.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newCapacity"/> is less than or equal to zero.</exception>
        public void EnsureCapacity(int newCapacity)
        {
            if (newCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(newCapacity), "Capacity must be greater than zero.");

            lock (_lock)
            {
                Capacity = newCapacity;
                TrimExcess();
            }
        }

        public int Count
        {
            get { lock (_lock) return _list.Count; }
        }

        public bool IsEmpty
        {
            get { lock (_lock) return _list.Count == 0; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Snapshot to avoid locking during enumeration
            T[] snapshot;
            lock (_lock)
            {
                snapshot = new T[_list.Count];
                _list.CopyTo(snapshot, 0);
            }
            return ((IEnumerable<T>)snapshot).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
