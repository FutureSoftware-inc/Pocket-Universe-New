// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a single node within a general-purpose hierarchical tree structure.
    /// </summary>
    /// <typeparam name="T">Specifies the type of data stored in the node.</typeparam>
    [DebuggerDisplay("Value = {Value}, ChildrenCount = {ChildrenCount}")]
    public class TreeNode<T>
    {
        private TreeNode<T> _parent;
        private TreeNode<T>[] _children;
        private int _childrenCount;
        private T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class with the specified value.
        /// </summary>
        public TreeNode(T value)
        {
            _value = value;
            _children = Array.Empty<TreeNode<T>>();
        }

        /// <summary>
        /// Gets or sets the value contained in this node.
        /// </summary>
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Gets the parent node of this node, or null if it is a root node.
        /// </summary>
        public TreeNode<T> Parent => _parent;

        /// <summary>
        /// Gets the number of direct child nodes.
        /// </summary>
        public int ChildrenCount => _childrenCount;

        /// <summary>
        /// Gets the internal array representing direct children. 
        /// For internal use and high-performance view proxies only.
        /// </summary>
        internal TreeNode<T>[] InternalChildren => _children;

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        public void AddChild(TreeNode<T> child)
        {
            child.ThrowIfNull(nameof(child));

            if (child._parent != null)
            {
                child._parent.RemoveChild(child);
            }

            if (_childrenCount == _children.Length)
            {
                GrowChildren(_childrenCount + 1);
            }

            child._parent = this;
            _children[_childrenCount++] = child;
        }

        /// <summary>
        /// Removes a specific child node from this node.
        /// </summary>
        public bool RemoveChild(TreeNode<T> child)
        {
            child.ThrowIfNull(nameof(child));

            int index = Array.IndexOf(_children, child, 0, _childrenCount);
            if (index < 0)
            {
                return false;
            }

            _childrenCount--;
            if (index < _childrenCount)
            {
                Array.Copy(_children, index + 1, _children, index, _childrenCount - index);
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<TreeNode<T>>())
            {
                _children[_childrenCount] = default!;
            }

            child._parent = null;
            return true;
        }

        private void GrowChildren(int minCapacity)
        {
            Debug.Assert(_children.Length < minCapacity);

            int newCapacity = Constants.GrowFactor * _children.Length;
            if ((uint)newCapacity > Constants.MaxLength)
            {
                newCapacity = Constants.MaxLength;
            }

            newCapacity = Math.Max(newCapacity, _children.Length + Constants.MinimumGrow);
            if (newCapacity < minCapacity)
            {
                newCapacity = minCapacity;
            }

            Array.Resize(ref _children, newCapacity);
        }

        /// <summary>
        /// Gets the child node at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the child node to get.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public TreeNode<T> this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_childrenCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, ErrorMessage.ArgumentOutOfRange.IndexMustBeLess);
                }
                return _children[index];
            }
        }

        /// <summary>
        /// Searches for a node containing the specified value within this node's subtree.
        /// </summary>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparer">The equality comparer to use, or null to use the default comparer.</param>
        public TreeNode<T> Find(T value, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            if (comparer.Equals(_value, value))
            {
                return this;
            }

            for (int i = 0; i < _childrenCount; i++)
            {
                TreeNode<T> found = _children[i].Find(value, comparer);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

    }
}