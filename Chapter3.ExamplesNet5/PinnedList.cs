using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Chapter3.Examples
{
    // TODO: use somehow GetPinnableReference to duck-type fixed properly (with no pinning as it is already pinned)

    /// <summary>
    /// List<typeparamref name="T"/> counterpart based on polled array living in the Pinned Object Heap.
    /// The array cannot contain any references 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PinnedList<T> : IList<T> where T : unmanaged 
    {
        private static readonly PinnedArrayPool<T> s_defaultPinnedArrayPool 
            = new PinnedArrayPool<T>(numberOfBuckets: 8, numberOfArraysPerBucket: 16);

        private PinnedArrayPool<T> _arrayPool;
        private T[] _items;
        private int _size;

        public PinnedList(int length = 4, PinnedArrayPool<T>? arrayPool = null)
        {
            // Avoid future failure of the GC.AllocateArray<T> call
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            _arrayPool = arrayPool ?? s_defaultPinnedArrayPool;
            _items = _arrayPool.Rent(length);
            _size = 0;
        }

        public T this[int index]
        {
            get
            {
                if (index >= _size)
                    ThrowArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                if (index >= _size)
                    ThrowArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
            }
        }

        public int Count => _size;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            T[] array = _items;
            int size = _size;
            if ((uint)size < (uint)array.Length)
            {
                _size = size + 1;
                array[size] = item;
            }
            else
            {
                AddWithResize(item);
            }
        }

        // Non-inline from List.Add to improve its code quality as uncommon path
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(T item)
        {
            int size = _size;
            EnsureCapacity(size + 1);
            _size = size + 1;
            _items[size] = item;
        }

        private void EnsureCapacity(int minimumCapacity)
        {
            if (_items.Length < minimumCapacity)
            {
                int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;
                if (newCapacity < minimumCapacity) newCapacity = minimumCapacity;
                ChangeCapacity(newCapacity);
            }
        }

        private void ChangeCapacity(int newCapacity)
        {
            if (newCapacity != _items.Length)
            {
                if (newCapacity > 0)
                {
                    T[] newItems = _arrayPool.Rent(newCapacity);
                    if (_size > 0)
                    {
                        Array.Copy(_items, newItems, _size);
                    }
                    _items = newItems;
                    _arrayPool.Return(_items);
                }
                else
                {
                    _items = Array.Empty<T>();
                }
            }
        }

        public void Clear()
        {
            // We do not need to clear elements as they do not contain references.
            // Also, do not return the array to the pool (assuming someone calls
            // Clear to reuse the list).
            _size = 0;
        }

        public bool Contains(T item)
        {
            return _size != 0 && IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException("Multi dimensional arrays are not supported");
            }
            try
            {
                // Array.Copy will check for NULL.
                Array.Copy(_items, 0, array!, arrayIndex, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("Invalid array type");
            }
        }

        public int IndexOf(T item)
            => Array.IndexOf(_items, item, 0, _size);

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private static void ThrowArgumentOutOfRangeException(string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName);
        private static void ThrowInvalidTypeWithPointersNotSupported(Type type) => 
            throw new ArgumentException($"{type.Name} must not be a reference type or a type that contains object references.");
    }
}
