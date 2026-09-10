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
    /// Represents a high-performance binary heap that can operate as either a Min-Heap or a Max-Heap.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the heap.</typeparam>
    [DebuggerDisplay("Count = {Count}, IsMaxHeap = {IsMaxHeap}")]
    [DebuggerTypeProxy(typeof(HeapDebugView<>))]
    public class Heap<T> : IReadOnlyCollection<T>, ICollection
    {
        private T[] _nodes;
        private int _size;
        private int _version;
        private readonly IComparer<T> _comparer;
        private readonly bool _isMaxHeap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Heap{T}"/> class (Min-Heap by default).
        /// </summary>
        public Heap(bool isMaxHeap = false)
        {
            _nodes = Array.Empty<T>();
            _isMaxHeap = isMaxHeap;
            _comparer = InitializeComparer(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Heap{T}"/> class with a specified capacity.
        /// </summary>
        public Heap(int initialCapacity, bool isMaxHeap = false)
        {
            initialCapacity.ThrowIfNegative(nameof(initialCapacity));

            _nodes = new T[initialCapacity];
            _isMaxHeap = isMaxHeap;
            _comparer = InitializeComparer(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Heap{T}"/> class with a custom comparer.
        /// </summary>
        public Heap(IComparer<T> comparer, bool isMaxHeap = false)
        {
            _nodes = Array.Empty<T>();
            _isMaxHeap = isMaxHeap;
            _comparer = InitializeComparer(comparer);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Heap{T}"/>.
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Gets a value indicating whether this heap is a Max-Heap. If false, it is a Min-Heap.
        /// </summary>
        public bool IsMaxHeap => _isMaxHeap;

        object ICollection.SyncRoot => this;
        bool ICollection.IsSynchronized => false;

        private static IComparer<T> InitializeComparer(IComparer<T> comparer)
        {
            if (typeof(T).IsValueType)
            {
                if (comparer == Comparer<T>.Default) return null;
                return comparer;
            }
            return comparer ?? Comparer<T>.Default;
        }

        /// <summary>
        /// Adds an element to the heap.
        /// </summary>
        /// <param name="item">The element to add to the heap.</param>
        public void Push(T item)
        {
            int currentSize = _size;
            _version++;

            if (_nodes.Length == currentSize)
            {
                Grow(currentSize + 1);
            }

            _size = currentSize + 1;

            if (_comparer == null)
            {
                MoveUpDefaultComparer(item, currentSize);
            }
            else
            {
                MoveUpCustomComparer(item, currentSize);
            }
        }

        /// <summary>
        /// Returns the root element from the heap without removing it.
        /// </summary>
        /// <returns>The element at the root of the heap (min or max depending on configuration).</returns>
        /// <exception cref="InvalidOperationException">The heap is empty.</exception>
        public T Peek()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyQueue);
            }

            return _nodes[0];
        }

        /// <summary>
        /// Returns the root element from the heap without removing it, if the heap is not empty.
        /// </summary>
        /// <param name="result">The root element if found; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if an element was found; otherwise, <see langword="false"/>.</returns>
        public bool TryPeek([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T result)
        {
            if (_size != 0)
            {
                result = _nodes[0];
                return true;
            }

            result = default;
            return false;
        }

        private void Grow(int minCapacity)
        {
            Debug.Assert(_nodes.Length < minCapacity);

            int newCapacity = Constants.GrowFactor * _nodes.Length;
            if ((uint)newCapacity > Constants.MaxLength)
            {
                newCapacity = Constants.MaxLength;
            }

            newCapacity = Math.Max(newCapacity, _nodes.Length + Constants.MinimumGrow);
            if (newCapacity < minCapacity)
            {
                newCapacity = minCapacity;
            }

            Array.Resize(ref _nodes, newCapacity);
        }

        private static int GetParentIndex(int index) => (index - 1) >> 1; // Двоичная куча: (i - 1) / 2

        private static int GetFirstChildIndex(int index) => (index << 1) + 1; // Двоичная куча: 2 * i + 1

        /// <summary>
        /// Removes and returns the root element from the heap.
        /// </summary>
        /// <returns>The element removed from the root of the heap.</returns>
        /// <exception cref="InvalidOperationException">The heap is empty.</exception>
        public T Pop()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyQueue);
            }

            T element = _nodes[0];
            RemoveRootNode();
            return element;
        }

        /// <summary>
        /// Removes and returns the root element from the heap, if the heap is not empty.
        /// </summary>
        /// <param name="result">The removed root element if successful; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if an element was removed; otherwise, <see langword="false"/>.</returns>
        public bool TryPop([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T result)
        {
            if (_size != 0)
            {
                result = _nodes[0];
                RemoveRootNode();
                return true;
            }

            result = default;
            return false;
        }

        private void RemoveRootNode()
        {
            int lastNodeIndex = --_size;
            _version++;

            if (lastNodeIndex > 0)
            {
                T lastNode = _nodes[lastNodeIndex];
                if (_comparer == null)
                {
                    MoveDownDefaultComparer(lastNode, 0);
                }
                else
                {
                    MoveDownCustomComparer(lastNode, 0);
                }
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _nodes[lastNodeIndex] = default!;
            }
        }

        private void MoveUpDefaultComparer(T node, int nodeIndex)
        {
            Debug.Assert(_comparer is null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            T[] nodes = _nodes;

            while (nodeIndex > 0)
            {
                int parentIndex = GetParentIndex(nodeIndex);
                T parent = nodes[parentIndex];

                int comp = Comparer<T>.Default.Compare(node, parent);

                // Если Max-Heap — инвертируем логику сравнения родителя и потомка
                if (_isMaxHeap) comp = -comp;

                if (comp < 0)
                {
                    nodes[nodeIndex] = parent;
                    nodeIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            nodes[nodeIndex] = node;
        }

        private void MoveUpCustomComparer(T node, int nodeIndex)
        {
            Debug.Assert(_comparer is not null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            IComparer<T> comparer = _comparer;
            T[] nodes = _nodes;

            while (nodeIndex > 0)
            {
                int parentIndex = GetParentIndex(nodeIndex);
                T parent = nodes[parentIndex];

                int comp = comparer.Compare(node, parent);
                if (_isMaxHeap) comp = -comp;

                if (comp < 0)
                {
                    nodes[nodeIndex] = parent;
                    nodeIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            nodes[nodeIndex] = node;
        }

        private void MoveDownDefaultComparer(T node, int nodeIndex)
        {
            Debug.Assert(_comparer is null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            T[] nodes = _nodes;
            int size = _size;

            int i;
            while ((i = GetFirstChildIndex(nodeIndex)) < size)
            {
                T selectedChild = nodes[i];
                int selectedChildIndex = i;

                int rightChildIndex = i + 1;
                if (rightChildIndex < size)
                {
                    T rightChild = nodes[rightChildIndex];
                    int comp = Comparer<T>.Default.Compare(rightChild, selectedChild);
                    if (_isMaxHeap) comp = -comp;

                    if (comp < 0)
                    {
                        selectedChild = rightChild;
                        selectedChildIndex = rightChildIndex;
                    }
                }

                int finalComp = Comparer<T>.Default.Compare(node, selectedChild);
                if (_isMaxHeap) finalComp = -finalComp;

                if (finalComp <= 0)
                {
                    break;
                }

                nodes[nodeIndex] = selectedChild;
                nodeIndex = selectedChildIndex;
            }

            nodes[nodeIndex] = node;
        }

        private void MoveDownCustomComparer(T node, int nodeIndex)
        {
            Debug.Assert(_comparer is not null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            IComparer<T> comparer = _comparer;
            T[] nodes = _nodes;
            int size = _size;

            int i;
            while ((i = GetFirstChildIndex(nodeIndex)) < size)
            {
                T selectedChild = nodes[i];
                int selectedChildIndex = i;

                int rightChildIndex = i + 1;
                if (rightChildIndex < size)
                {
                    T rightChild = nodes[rightChildIndex];
                    int comp = comparer.Compare(rightChild, selectedChild);
                    if (_isMaxHeap) comp = -comp;

                    if (comp < 0)
                    {
                        selectedChild = rightChild;
                        selectedChildIndex = rightChildIndex;
                    }
                }

                int finalComp = comparer.Compare(node, selectedChild);
                if (_isMaxHeap) finalComp = -finalComp;

                if (finalComp <= 0)
                {
                    break;
                }

                nodes[nodeIndex] = selectedChild;
                nodeIndex = selectedChildIndex;
            }

            nodes[nodeIndex] = node;
        }

        /// <summary>
        /// Removes all items from the <see cref="Heap{T}"/>.
        /// </summary>
        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_nodes, 0, _size);
            }
            _size = 0;
            _version++;
        }

        /// <summary>
        /// Ensures that the <see cref="Heap{T}"/> can hold up to <paramref name="capacity"/> items without further expansion.
        /// </summary>
        public int EnsureCapacity(int capacity)
        {
            capacity.ThrowIfNegative(nameof(capacity));

            if (_nodes.Length < capacity)
            {
                Grow(capacity);
                _version++;
            }

            return _nodes.Length;
        }

        /// <summary>
        /// Sets the capacity to the actual number of items in the <see cref="Heap{T}"/>, if less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            int threshold = (int)(_nodes.Length * 0.9);
            if (_size < threshold)
            {
                Array.Resize(ref _nodes, _size);
                _version++;
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="Heap{T}"/> to a new array.
        /// </summary>
        public T[] ToArray()
        {
            if (_size == 0)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[_size];
            Array.Copy(_nodes, 0, result, 0, _size);
            return result;
        }

        /// <summary>
        /// Copies the elements of the <see cref="Heap{T}"/> to an <see cref="Array"/>, starting at a particular index.
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

            try
            {
                Array.Copy(_nodes, 0, array, index, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(ErrorMessage.Argument.IncompatibleArrayType, nameof(array));
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Heap{T}"/> in an unordered manner.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            _size == 0 ? EnumerableHelpers.GetEmptyEnumerator<T>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        /// <summary>
        /// Enumerates the elements of a <see cref="Heap{T}"/> without any ordering guarantees.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly Heap<T> _heap;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(Heap<T> heap)
            {
                _heap = heap;
                _version = heap._version;
                _index = 0;
                _current = default!;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                Heap<T> localHeap = _heap;

                if (_version == localHeap._version && ((uint)_index < (uint)localHeap._size))
                {
                    _current = localHeap._nodes[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _heap._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _index = _heap._size + 1;
                _current = default!;
                return false;
            }

            public T Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                if (_version != _heap._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _index = 0;
                _current = default!;
            }
        }
    }
}