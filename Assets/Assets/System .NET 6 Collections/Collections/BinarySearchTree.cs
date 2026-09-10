// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, iterative Binary Search Tree (BST).
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the tree.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(BinarySearchTreeDebugView<>))]
    public class BinarySearchTree<T> : IReadOnlyCollection<T>, ICollection
    {
        private BSTNode<T> _root;
        private int _size;
        private int _version;
        private readonly IComparer<T> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySearchTree{T}"/> class.
        /// </summary>
        public BinarySearchTree() : this(comparer: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySearchTree{T}"/> class with a custom comparer.
        /// </summary>
        public BinarySearchTree(IComparer<T> comparer)
        {
            _comparer = comparer ?? Comparer<T>.Default;
        }

        /// <summary>
        /// Gets the total number of elements contained in the tree.
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Gets the root node of the Binary Search Tree.
        /// </summary>
        public BSTNode<T> Root => _root;

        object ICollection.SyncRoot => this;
        bool ICollection.IsSynchronized => false;

        /// <summary>
        /// Iteratively calculates the maximum height (depth) of the tree.
        /// </summary>
        public int Height
        {
            get
            {
                if (_root == null) return 0;

                // Для итеративного подсчета высоты используем обход в ширину (BFS) 
                // на базе написанной нами ранее двусторонней очереди Deque
                var queue = new Deque<BSTNode<T>>();
                queue.AddLast(_root);
                int height = 0;

                while (queue.Count > 0)
                {
                    int nodeCount = queue.Count;
                    height++;

                    // Сбрасываем весь текущий уровень дерева в очередь
                    while (nodeCount > 0)
                    {
                        BSTNode<T> node = queue.RemoveFirst();
                        if (node._left != null) queue.AddLast(node._left);
                        if (node._right != null) queue.AddLast(node._right);
                        nodeCount--;
                    }
                }

                return height;
            }
        }

        /// <summary>
        /// Determines whether the tree contains a specific value.
        /// </summary>
        public bool Contains(T value)
        {
            BSTNode<T> current = _root;
            IComparer<T> comparer = _comparer;

            while (current != null)
            {
                int compare = comparer.Compare(value, current._value);

                if (compare == 0)
                {
                    return true;
                }

                current = compare < 0 ? current._left : current._right;
            }

            return false;
        }

        /// <summary>
        /// Adds an element to the Binary Search Tree. 
        /// If the element already exists, the tree remains unchanged.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns><see langword="true"/> if the element was successfully added; <see langword="false"/> if it already exists.</returns>
        public bool Add(T item)
        {
            if (_root == null)
            {
                _root = new BSTNode<T>(item);
                _size++;
                _version++;
                return true;
            }

            BSTNode<T> current = _root;
            IComparer<T> comparer = _comparer;

            while (true)
            {
                int compare = comparer.Compare(item, current._value);

                if (compare == 0)
                {
                    // Элемент уже присутствует в дереве. Дубликаты не поддерживаются.
                    return false;
                }

                if (compare < 0)
                {
                    if (current._left == null)
                    {
                        current._left = new BSTNode<T>(item);
                        break;
                    }
                    current = current._left;
                }
                else
                {
                    if (current._right == null)
                    {
                        current._right = new BSTNode<T>(item);
                        break;
                    }
                    current = current._right;
                }
            }

            _size++;
            _version++;
            return true;
        }

        /// <summary>
        /// Removes all elements from the tree.
        /// </summary>
        public void Clear()
        {
            // Корневой узел зануляется, позволяя GC рекурсивно собрать всю иерархию поддеревьев
            _root = null;
            _size = 0;
            _version++;
        }

        /// <summary>
        /// Removes a specific value from the Binary Search Tree.
        /// </summary>
        /// <param name="item">The value to remove.</param>
        /// <returns><see langword="true"/> if the item was successfully removed; <see langword="false"/> if it was not found.</returns>
        public bool Remove(T item)
        {
            BSTNode<T> parent = null;
            BSTNode<T> current = _root;
            IComparer<T> comparer = _comparer;

            // Шаг 1: Итеративно ищем удаляемый узел и его родителя
            while (current != null)
            {
                int compare = comparer.Compare(item, current._value);
                if (compare == 0) break;

                parent = current;
                current = compare < 0 ? current._left : current._right;
            }

            if (current == null)
            {
                return false; // Элемент не найден
            }

            // Шаг 2: Узлы найдены, запускаем логику удаления
            _version++;
            _size--;

            // Сценарий А и Б: У узла 0 или 1 потомок
            if (current._left == null || current._right == null)
            {
                BSTNode<T> newChild = current._left ?? current._right;

                if (parent == null)
                {
                    _root = newChild; // Удаляем корень дерева
                }
                else if (parent._left == current)
                {
                    parent._left = newChild;
                }
                else
                {
                    parent._right = newChild;
                }
            }
            // Сценарий В: У узла ДВА потомка
            else
            {
                // Ищем самого левого (минимального) потомка в правом поддереве
                BSTNode<T> successorParent = current;
                BSTNode<T> successor = current._right;

                while (successor._left != null)
                {
                    successorParent = successor;
                    successor = successor._left;
                }

                // Копируем значение преемника в текущий узел
                current._value = successor._value;

                // Отсоединяем преемника от его родителя
                if (successorParent._left == successor)
                {
                    successorParent._left = successor._right;
                }
                else
                {
                    successorParent._right = successor._right;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the minimum value contained in the tree.
        /// </summary>
        /// <exception cref="InvalidOperationException">The tree is empty.</exception>
        public T Min()
        {
            if (_root == null)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyQueue);
            }

            BSTNode<T> current = _root;
            while (current._left != null)
            {
                current = current._left;
            }

            return current._value;
        }

        /// <summary>
        /// Returns the maximum value contained in the tree.
        /// </summary>
        /// <exception cref="InvalidOperationException">The tree is empty.</exception>
        public T Max()
        {
            if (_root == null)
            {
                throw new InvalidOperationException(ErrorMessage.InvalidOperation.EmptyQueue);
            }

            BSTNode<T> current = _root;
            while (current._right != null)
            {
                current = current._right;
            }

            return current._value;
        }

        /// <summary>
        /// Searches for a specific value and returns its node.
        /// </summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>The node if found; otherwise, null.</returns>
        public BSTNode<T> FindNode(T value)
        {
            BSTNode<T> current = _root;
            IComparer<T> comparer = _comparer;

            while (current != null)
            {
                int compare = comparer.Compare(value, current._value);
                if (compare == 0)
                {
                    return current;
                }
                current = compare < 0 ? current._left : current._right;
            }

            return null;
        }

        /// <summary>
        /// Copies the sorted elements of the Binary Search Tree to a contiguous array, starting at a particular index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, ErrorMessage.ArgumentOutOfRange.IndexMustBeLessOrEqual);
            }

            if (array.Length - arrayIndex < _size)
            {
                throw new ArgumentException(ErrorMessage.Argument.InvalidOffLen);
            }

            if (_size == 0) return;

            // Используем наш zero-allocation итератор для копирования данных
            foreach (T item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        void ICollection.CopyTo(Array array, int index)
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
                int currentIndex = index;
                foreach (T item in this)
                {
                    array.SetValue(item, currentIndex++);
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(ErrorMessage.Argument.IncompatibleArrayType, nameof(array));
            }
        }

        /// <summary>
        /// Converts the Binary Search Tree elements into a sorted contiguous array.
        /// </summary>
        public T[] ToArray()
        {
            if (_size == 0)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[_size];
            CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Binary Search Tree in sorted order.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            _size == 0 ? EnumerableHelpers.GetEmptyEnumerator<T>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        /// <summary>
        /// Enumerates the elements of a <see cref="BinarySearchTree{T}"/> in sorted (in-order) sequence.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly BinarySearchTree<T> _tree;
            private readonly int _version;
            private Stack<BSTNode<T>> _stack;
            private BSTNode<T> _current;
            private T _currentValue;

            internal Enumerator(BinarySearchTree<T> tree)
            {
                _tree = tree;
                _version = tree._version;
                _current = tree.Root;
                _currentValue = default!;

                if (tree.Root != null)
                {
                    _stack = new Stack<BSTNode<T>>();
                    PushLeftNodes(_current);
                }
                else
                {
                    _stack = null;
                }
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (_version != _tree._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                if (_stack == null || _stack.Count == 0)
                {
                    _currentValue = default!;
                    return false;
                }

                // Извлекаем узел из стека (он гарантированно минимальный на данный момент)
                BSTNode<T> node = _stack.Pop();
                _currentValue = node._value;

                // Если у узла есть правое поддерево, уходим в него и спускаемся до упора влево
                if (node._right != null)
                {
                    PushLeftNodes(node._right);
                }

                return true;
            }

            private void PushLeftNodes(BSTNode<T> node)
            {
                Debug.Assert(_stack != null);
                while (node != null)
                {
                    _stack.Push(node);
                    node = node._left;
                }
            }

            public T Current => _currentValue;

            object IEnumerator.Current => _currentValue;

            void IEnumerator.Reset()
            {
                if (_version != _tree._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _currentValue = default!;
                if (_tree.Root != null)
                {
                    _stack ??= new Stack<BSTNode<T>>();
                    _stack.Clear();
                    PushLeftNodes(_tree.Root);
                }
                else
                {
                    _stack = null;
                }
            }
        }
    }
}