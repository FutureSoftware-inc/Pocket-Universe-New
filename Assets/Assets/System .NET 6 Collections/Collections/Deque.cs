// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a double-ended queue (deque) backed by a circular array.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the deque.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DequeDebugView<>))]
    public class Deque<T> : IReadOnlyCollection<T>, ICollection
    {
        private T[] _array;
        private int _head;
        private int _tail;
        private int _size;
        private int _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that is empty.
        /// </summary>
        public Deque()
        {
            _array = Array.Empty<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the deque.</param>
        public Deque(int capacity)
        {
            capacity.ThrowIfNegative(nameof(capacity));
            _array = new T[capacity];
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Deque{T}"/>.
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Gets the current capacity of the underlying internal array.
        /// </summary>
        public int Capacity => _array.Length;

        object ICollection.SyncRoot => this;
        bool ICollection.IsSynchronized => false;

        /// <summary>
        /// Adds an item to the front of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddFirst(T item)
        {
            if (_size == _array.Length)
            {
                Grow(_size + 1);
            }

            // Сдвигаем указатель головы влево по кольцу.
            // Если выходим за 0, перемещаемся в конец массива.
            _head = _head == 0 ? _array.Length - 1 : _head - 1;
            _array[_head] = item;

            _size++;
            _version++;
        }

        /// <summary>
        /// Adds an item to the back of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddLast(T item)
        {
            if (_size == _array.Length)
            {
                Grow(_size + 1);
            }

            // Добавляем элемент в текущую позицию хвоста,
            // затем сдвигаем указатель хвоста вправо по кольцу.
            _array[_tail] = item;
            _tail = (_tail + 1) == _array.Length ? 0 : _tail + 1;

            _size++;
            _version++;
        }

        /// <summary>
        /// Grows the internal array to match the specified minimum capacity.
        /// </summary>
        private void Grow(int minCapacity)
        {
            Debug.Assert(_array.Length < minCapacity);

            int newCapacity = Constants.GrowFactor * _array.Length;

            if ((uint)newCapacity > Constants.MaxLength)
            {
                newCapacity = Constants.MaxLength;
            }

            newCapacity = Math.Max(newCapacity, _array.Length + Constants.MinimumGrow);
            if (newCapacity < minCapacity)
            {
                newCapacity = minCapacity;
            }

            SetCapacity(newCapacity);
        }

        /// <summary>
        /// Allocates a new array and copies the circular buffer elements into a contiguous layout.
        /// </summary>
        private void SetCapacity(int capacity)
        {
            T[] newArray = new T[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    // Элементы лежат в массиве непрерывно
                    Array.Copy(_array, _head, newArray, 0, _size);
                }
                else
                {
                    // Элементы разделены (закольцованы) на две части в памяти
                    Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
                }
            }

            _array = newArray;
            _head = 0;
            _tail = _size == capacity ? 0 : _size;
            _version++;
        }

        /// <summary>
        /// Returns the item at the front of the <see cref="Deque{T}"/> without removing it.
        /// </summary>
        /// <returns>The item at the front of the <see cref="Deque{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        public T PeekFirst()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyDeque);
            }

            return _array[_head];
        }

        /// <summary>
        /// Returns the item at the back of the <see cref="Deque{T}"/> without removing it.
        /// </summary>
        /// <returns>The item at the back of the <see cref="Deque{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        public T PeekLast()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyDeque);
            }

            int lastIndex = _tail == 0 ? _array.Length - 1 : _tail - 1;
            return _array[lastIndex];
        }

        /// <summary>
        /// Removes and returns the item at the front of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The item removed from the front of the <see cref="Deque{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        public T RemoveFirst()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyDeque);
            }

            T removed = _array[_head];

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _array[_head] = default!;
            }

            _head = (_head + 1) == _array.Length ? 0 : _head + 1;
            _size--;
            _version++;

            return removed;
        }

        /// <summary>
        /// Removes and returns the item at the back of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The item removed from the back of the <see cref="Deque{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        public T RemoveLast()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyDeque);
            }

            int lastIndex = _tail == 0 ? _array.Length - 1 : _tail - 1;
            T removed = _array[lastIndex];

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _array[lastIndex] = default!;
            }

            _tail = lastIndex;
            _size--;
            _version++;

            return removed;
        }

        /// <summary>
        /// Removes all items from the <see cref="Deque{T}"/>.
        /// </summary>
        public void Clear()
        {
            if (_size > 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    if (_head < _tail)
                    {
                        Array.Clear(_array, _head, _size);
                    }
                    else
                    {
                        Array.Clear(_array, _head, _array.Length - _head);
                        Array.Clear(_array, 0, _tail);
                    }
                }

                _head = 0;
                _tail = 0;
                _size = 0;
            }

            _version++;
        }

        /// <summary>
        /// Gets or sets the element at the specified virtual index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than the collection size.</exception>
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, ErrorMessage.ArgumentOutOfRange.IndexMustBeLess);
                }

                int bufferIndex = _head + index;
                if (bufferIndex >= _array.Length)
                {
                    bufferIndex -= _array.Length;
                }
                return _array[bufferIndex];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, ErrorMessage.ArgumentOutOfRange.IndexMustBeLess);
                }

                int bufferIndex = _head + index;
                if (bufferIndex >= _array.Length)
                {
                    bufferIndex -= _array.Length;
                }
                _array[bufferIndex] = value;
                _version++;
            }
        }

        /// <summary>
        /// Ensures that the <see cref="Deque{T}"/> can hold up to <paramref name="capacity"/> items without expansion.
        /// </summary>
        public int EnsureCapacity(int capacity)
        {
            capacity.ThrowIfNegative(nameof(capacity));

            if (_array.Length < capacity)
            {
                Grow(capacity);
            }

            return _array.Length;
        }

        /// <summary>
        /// Sets the capacity to the actual number of items in the <see cref="Deque{T}"/>, if less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            int threshold = (int)(_array.Length * 0.9);
            if (_size < threshold)
            {
                SetCapacity(_size);
            }
        }

        /// <summary>
        /// Copies the <see cref="Deque{T}"/> elements to a new array.
        /// </summary>
        public T[] ToArray()
        {
            if (_size == 0)
            {
                return Array.Empty<T>();
            }

            T[] localArray = new T[_size];
            CopyTo(localArray, 0);
            return localArray;
        }

        /// <summary>
        /// Copies the elements of the <see cref="Deque{T}"/> to an <see cref="Array"/>, starting at a particular index.
        /// </summary>
        public void CopyTo(Array array, int index)
        {
            array.ThrowIfNull(nameof(array));

            if (array.Rank != 1)
            {
                throw new ArgumentException(ErrorMessage.Argument.RankMultiDimNotSupported, nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(ErrorMessage.Argument.NonZeroLowerBound, nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, ErrorMessage.ArgumentOutOfRange.IndexMustBeLessOrEqual);
            }

            if (array.Length - index < _size)
            {
                throw new ArgumentException(ErrorMessage.Argument.InvalidOffLen);
            }

            if (_size == 0)
            {
                return;
            }

            try
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, array, index, _size);
                }
                else
                {
                    Array.Copy(_array, _head, array, index, _array.Length - _head);
                    Array.Copy(_array, 0, array, index + (_array.Length - _head), _tail);
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(ErrorMessage.Argument.IncompatibleArrayType, nameof(array));
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Deque{T}"/>.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            _size == 0 ? EnumerableHelpers.GetEmptyEnumerator<T>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        /// <summary>
        /// Enumerates the elements of a <see cref="Deque{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly Deque<T> _deque;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(Deque<T> deque)
            {
                _deque = deque;
                _version = deque._version;
                _index = 0;
                _current = default!;
            }

            /// <summary>
            /// Releases all resources used by the <see cref="Enumerator"/>.
            /// </summary>
            public void Dispose() { }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="Deque{T}"/>.
            /// </summary>
            public bool MoveNext()
            {
                Deque<T> localDeque = _deque;

                if (_version == localDeque._version && (uint)_index < (uint)localDeque._size)
                {
                    int bufferIndex = localDeque._head + _index;
                    if (bufferIndex >= localDeque._array.Length)
                    {
                        bufferIndex -= localDeque._array.Length;
                    }

                    _current = localDeque._array[bufferIndex];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _deque._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _index = _deque._size + 1;
                _current = default!;
                return false;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                if (_version != _deque._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _index = 0;
                _current = default!;
            }
        }
    }
}