// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a single node within a Binary Search Tree (BST).
    /// </summary>
    /// <typeparam name="T">Specifies the type of data stored in the node.</typeparam>
    [DebuggerDisplay("Value = {Value}")]
    public class BSTNode<T>
    {
        internal T _value;
        internal BSTNode<T> _left;
        internal BSTNode<T> _right;

        /// <summary>
        /// Initializes a new instance of the <see cref="BSTNode{T}"/> class with the specified value.
        /// </summary>
        public BSTNode(T value)
        {
            _value = value;
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
        /// Gets the left child node.
        /// </summary>
        public BSTNode<T> Left => _left;

        /// <summary>
        /// Gets the right child node.
        /// </summary>
        public BSTNode<T> Right => _right;
    }
}