using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chapter3.Examples
{
    /// <summary>
    /// ArrayPool<typeparamref name="T"/> re-implementation reusing arrays from the Pinned Object Heap.
    /// Currently it is a simple modified ConfigurableArrayPool<typeparamref name="T"/> code with
    /// some refactorings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PinnedArrayPool<T> : ArrayPool<T> where T : unmanaged
    {
        private readonly Bucket[] _buckets;

        public PinnedArrayPool(int numberOfBuckets, int numberOfArraysPerBucket)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                ThrowInvalidTypeWithPointersNotSupported(typeof(T));

            var buckets = new Bucket[numberOfBuckets + 1];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new Bucket(GetArraySizeForBucket(i), numberOfArraysPerBucket);
            }
            _buckets = buckets;
        }


        public override T[] Rent(int minimumLength)
        {
            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }
            else if (minimumLength == 0)
            {
                return Array.Empty<T>();
            }
            T[]? result;
            int index = SelectBucketIndex(minimumLength);
            if (index < _buckets.Length)
            {
                const int MaxBucketsToTry = 2;
                int i = index;
                do
                {
                    result = _buckets[i].Rent();
                    if (result != null)
                    {
                        return result;
                    }
                }
                while (++i < _buckets.Length && i != index + MaxBucketsToTry);

                // The pool was exhausted for this buffer size.  Allocate a new buffer with a size corresponding
                // to the appropriate bucket.
                result = GC.AllocateArray<T>(_buckets[index]._arrayLength, pinned: true);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }
            return result;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            else if (array.Length == 0)
            {
                return;
            }
            // Waiting for https://github.com/dotnet/runtime/issues/33542 but BE concerned about perf-overhead
            // if (!GC.IsPinnedHeapObject(array))
            //    throw new ArgumentException("Trying to return array outside of the POH");

            int bucket = SelectBucketIndex(array.Length);
            if (bucket < _buckets.Length)
            {
                // Clear the array if the user requests
                if (clearArray)
                {
                    Array.Clear(array, 0, array.Length);
                }
                _buckets[bucket].Return(array);
            }
        }

        private static int GetArraySizeForBucket(int i) => (int)BitOperations.RotateLeft(2U, i + 2); // 
        private static int SelectBucketIndex(int size) => BitOperations.Log2((uint)size) - 2;

        private static void ThrowInvalidTypeWithPointersNotSupported(Type type)
            => throw new ArgumentException($"{type.Name} must not be a reference type or a type that contains object references.");

        /// <summary>Provides a thread-safe bucket containing buffers that can be Rent'd and Return'd.</summary>
        private sealed class Bucket
        {
            internal readonly int _arrayLength;
            private readonly T[]?[] _arrays;

            private SpinLock _lock; // TODO: Add Resharper exception
            private int _index;

            internal Bucket(int arrayLength, int numberOfArrays)
            {
                _lock = new SpinLock(Debugger.IsAttached);
                _arrays = new T[numberOfArrays][];
                _arrayLength = arrayLength;
            }

            internal T[]? Rent()
            {
                T[]?[] arrays = _arrays;
                T[]? result = null;
                bool lockTaken = false;
                bool allocateArray = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_index < arrays.Length)
                    {
                        result = arrays[_index];
                        arrays[_index++] = null;
                        allocateArray = result == null;
                    }
                }
                finally
                {
                    if (lockTaken) _lock.Exit(false);
                }
                if (allocateArray)
                {
                    result = GC.AllocateArray<T>(_arrayLength, pinned: true);
                }
                return result;
            }

            internal void Return(T[] array)
            {
                // Check to see if the buffer is the correct size for this bucket
                if (array.Length != _arrayLength)
                {
                    throw new ArgumentException($"Invalid size of the array returned to the bucket of {nameof(PinnedArrayPool<T>)}");
                }
                bool lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_index != 0)
                    {
                        _arrays[--_index] = array;
                    }
                }
                finally
                {
                    if (lockTaken) _lock.Exit(false);
                }
            }
        }
    }
}
