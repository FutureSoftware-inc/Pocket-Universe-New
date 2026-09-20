// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Specifies the traversal strategy for a hierarchical tree structure.
    /// </summary>
    public enum TreeTraversalStrategy
    {
        /// <summary>
        /// Breadth-First Search (BFS) / Обход в ширину.
        /// </summary>
        BreadthFirst,

        /// <summary>
        /// Depth-First Search (DFS) Pre-Order / Обход в глубину.
        /// </summary>
        DepthFirst
    }

    /// <summary>
    /// Represents a general-purpose hierarchical tree data structure.
    /// </summary>
    /// <typeparam name="T">Specifies the type of data stored in the tree nodes.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TreeDebugView<>))]
    public class Tree<T> : IEnumerable<TreeNode<T>>
    {
        private TreeNode<T> _root;
        private int _version;
        private TreeTraversalStrategy _traversalStrategy = TreeTraversalStrategy.BreadthFirst;


        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T}"/> class that is empty.
        /// </summary>
        public Tree()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{T}"/> class with the specified root node.
        /// </summary>
        public Tree(TreeNode<T> root)
        {
            root.ThrowIfNull(nameof(root));
            _root = root;
        }

        /// <summary>
        /// Gets or sets the root node of the tree.
        /// </summary>
        public TreeNode<T> Root
        {
            get => _root;
            set
            {
                _root = value;
                _version++;
            }
        }

        /// <summary>
        /// Gets the total number of nodes inside this tree.
        /// </summary>
        public int Count => _root == null ? 0 : CountNodes(_root);

        /// <summary>
        /// Gets or sets the default traversal strategy used by foreach loops on this tree.
        /// </summary>
        public TreeTraversalStrategy TraversalStrategy
        {
            get => _traversalStrategy;
            set => _traversalStrategy = value;
        }

        internal int Version => _version;

        /// <summary>
        /// Returns an enumerator that iterates through the tree using the current <see cref="TraversalStrategy"/>.
        /// </summary>
        public TreeEnumerator GetEnumerator() => new TreeEnumerator(this, _traversalStrategy);

        IEnumerator<TreeNode<T>> IEnumerable<TreeNode<T>>.GetEnumerator() =>
            _root == null ? EnumerableHelpers.GetEmptyEnumerator<TreeNode<T>>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TreeNode<T>>)this).GetEnumerator();

        /// <summary>
        /// Removes the specified node from the tree. 
        /// If the node is the root, the tree becomes empty.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns><see langword="true"/> if the node was successfully removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(TreeNode<T> node)
        {
            node.ThrowIfNull(nameof(node));
            if (_root == node)
            {
                _root = null;
                _version++;
                return true;
            }
            if (node.Parent != null)
            {
                bool removed = node.Parent.RemoveChild(node);
                if (removed)
                {
                    _version++;
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of the tree (as TreeNodes) to a contiguous array, 
        /// following the current <see cref="TraversalStrategy"/>.
        /// </summary>
        public void CopyTo(TreeNode<T>[] array, int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));
            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, ErrorMessage.ArgumentOutOfRange.IndexMustBeLessOrEqual);
            }
            int count = Count;
            if (array.Length - arrayIndex < count)
            {
                throw new ArgumentException(ErrorMessage.Argument.InvalidOffLen);
            }
            foreach (TreeNode<T> node in this)
            {
                array[arrayIndex++] = node;
            }
        }

        /// <summary>
        /// Converts the tree hierarchy into a flat array of nodes 
        /// based on the current <see cref="TraversalStrategy"/>.
        /// </summary>
        public TreeNode<T>[] ToArray()
        {
            int count = Count;
            if (count == 0)
            {
                return Array.Empty<TreeNode<T>>();
            }
            TreeNode<T>[] result = new TreeNode<T>[count];
            CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Clears the entire tree by detaching the root node.
        /// </summary>
        public void Clear()
        {
            _root = null;
            _version++;
        }

        private static int CountNodes(TreeNode<T> node)
        {
            int count = 1;
            TreeNode<T>[] children = node.InternalChildren;
            int childrenCount = node.ChildrenCount;
            for (int i = 0; i < childrenCount; i++)
            {
                count += CountNodes(children[i]);
            }
            return count;
        }

        /// <summary>
        /// A unified zero-allocation enumerator that dispatches rendering to BFS or DFS strategies.
        /// </summary>
        public struct TreeEnumerator : IEnumerator<TreeNode<T>>
        {
            private readonly TreeTraversalStrategy _strategy;
            private BreadthFirstEnumerator _bfsEnumerator;
            private DepthFirstEnumerator _dfsEnumerator;

            internal TreeEnumerator(Tree<T> tree, TreeTraversalStrategy strategy)
            {
                _strategy = strategy;
                if (strategy == TreeTraversalStrategy.BreadthFirst)
                {
                    _bfsEnumerator = new BreadthFirstEnumerator(tree);
                    _dfsEnumerator = default;
                }
                else
                {
                    _bfsEnumerator = default;
                    _dfsEnumerator = new DepthFirstEnumerator(tree);
                }
            }

            public bool MoveNext()
            {
                return _strategy == TreeTraversalStrategy.BreadthFirst ? _bfsEnumerator.MoveNext() : _dfsEnumerator.MoveNext();
            }

            public TreeNode<T> Current
            {
                get
                {
                    return _strategy == TreeTraversalStrategy.BreadthFirst ? _bfsEnumerator.Current : _dfsEnumerator.Current;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_strategy == TreeTraversalStrategy.BreadthFirst)
                    _bfsEnumerator.Dispose();
                else
                    _dfsEnumerator.Dispose();
            }

            void IEnumerator.Reset()
            {
                if (_strategy == TreeTraversalStrategy.BreadthFirst)
                    _bfsEnumerator.Reset();
                else
                    _dfsEnumerator.Reset();
            }
        }

        /// <summary>
        /// Infrastructure wrapper to provide DFS capability via Linq/foreach.
        /// </summary>
        public readonly struct DepthFirstEnumerable : IEnumerable<TreeNode<T>>
        {
            private readonly Tree<T> _tree;

            internal DepthFirstEnumerable(Tree<T> tree) => _tree = tree;

            public DepthFirstEnumerator GetEnumerator() => new DepthFirstEnumerator(_tree);

            IEnumerator<TreeNode<T>> IEnumerable<TreeNode<T>>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        public struct BreadthFirstEnumerator : IEnumerator<TreeNode<T>>
        {
            private readonly Tree<T> _tree;
            private readonly int _version;
            private Deque<TreeNode<T>> _queue;
            private TreeNode<T> _current;

            internal BreadthFirstEnumerator(Tree<T> tree)
            {
                _tree = tree;
                _version = tree.Version;
                _current = null;
                if (tree.Root != null)
                {
                    _queue = new Deque<TreeNode<T>>();
                    _queue.AddLast(tree.Root);
                }
                else
                {
                    _queue = null;
                }
            }

            public bool MoveNext()
            {
                if (_version != _tree.Version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }
                if (_queue == null || _queue.Count == 0)
                {
                    _current = null;
                    return false;
                }
                _current = _queue.RemoveFirst();
                TreeNode<T>[] children = _current.InternalChildren;
                int count = _current.ChildrenCount;
                for (int i = 0; i < count; i++)
                {
                    _queue.AddLast(children[i]);
                }
                return true;
            }

            public TreeNode<T> Current => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public void Reset()
            {
                if (_version != _tree.Version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }
                _current = null;
                if (_tree.Root != null)
                {
                    _queue = new Deque<TreeNode<T>>();
                    _queue.AddLast(_tree.Root);
                }
                else
                {
                    _queue = null;
                }
            }
        }

        /// <summary>
        /// Enumerates the tree nodes using Depth-First Search (DFS) pre-order strategy.
        /// </summary>
        public struct DepthFirstEnumerator : IEnumerator<TreeNode<T>>
        {
            private readonly Tree<T> _tree;
            private readonly int _version;
            private Stack<TreeNode<T>> _stack;
            private TreeNode<T> _current;

            internal DepthFirstEnumerator(Tree<T> tree)
            {
                _tree = tree;
                _version = tree.Version;
                _current = null;
                if (tree.Root != null)
                {
                    _stack = new Stack<TreeNode<T>>();
                    _stack.Push(tree.Root);
                }
                else
                {
                    _stack = null;
                }
            }

            public bool MoveNext()
            {
                if (_version != _tree.Version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }
                if (_stack == null || _stack.Count == 0)
                {
                    _current = null;
                    return false;
                }
                _current = _stack.Pop();
                TreeNode<T>[] children = _current.InternalChildren;
                int count = _current.ChildrenCount;
                for (int i = count - 1; i >= 0; i--)
                {
                    _stack.Push(children[i]);
                }
                return true;
            }

            public TreeNode<T> Current => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public void Reset()
            {
                if (_version != _tree.Version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }
                _current = null;
                if (_tree.Root != null)
                {
                    _stack = new Stack<TreeNode<T>>();
                    _stack.Push(_tree.Root);
                }
                else
                {
                    _stack = null;
                }
            }
        }
    }
}