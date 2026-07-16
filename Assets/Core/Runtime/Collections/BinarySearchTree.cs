using System;
using System.Collections;
using System.Collections.Generic;

namespace Crystal.Collections

{
    public class BinaryTreeNode<T> where T : IComparable<T>
    {
        public T Value { get; set; }
        public BinaryTreeNode<T> Left { get; set; }
        public BinaryTreeNode<T> Right { get; set; }

        public BinaryTreeNode(T value)
        {
            Value = value;
        }
    }

    public class BinarySearchTree<T> : ICollection<T> where T : IComparable<T>
    {
        private BinaryTreeNode<T> _root;
        private int _size;

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            if (_root == null)
            {
                _root = new BinaryTreeNode<T>(item);
            }
            else
            {
                Add(_root, item);
            }
        }

        public bool Remove(T item)
        {
            if (!Contains(item))
            {
                return false;
            }
            return Remove(_root, item);
        }

        public bool Contains(T item)
        {
            return Contains(_root, item);
        }

        // Метод для обхода дерева в порядке возрастания (ин-ордер)
        public void InOrderTraversal(Action<T> action)
        {
            InOrderTraversal(_root, action);
        }


        private void Add(BinaryTreeNode<T> current, T item)
        {
            if (item.CompareTo(current.Value) < 0)
            {
                if (current.Left == null)
                {
                    current.Left = new BinaryTreeNode<T>(item);
                }
                else
                {
                    Add(current.Left, item);
                }
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new BinaryTreeNode<T>(item);
                }
                else
                {
                    Add(current.Right, item);
                }
            }
        }

        private bool Remove(BinaryTreeNode<T> current, T item)
        {
            if (item.CompareTo(current.Value) == 0)
            {
                if (current.Left == null || current.Right == null)
                {
                    current = current.Left == null ? current.Right : current.Left;
                }
                else
                {
                    T maxInLeft = Max(current.Left);
                    current.Value = maxInLeft;
                    Remove(current.Right, maxInLeft);
                }
                return true;
            }
            return item.CompareTo(current.Value) < 0 ? Remove(current.Left, item) : Remove(current.Right, item);
        }

        public T Min()
        {
            return Min(_root);
        }

        public T Max()
        {
            return Max(_root);
        }

        public void Clear()
        {
            _root = null;
        }

        public void CopyTo(T[] targetArray, int arrayIndex)
        {
            if (targetArray == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0 || arrayIndex >= targetArray.Length)
            {
                throw new ArgumentOutOfRangeException("array");
            }
            int targetLenght = targetArray.Length;
            if (targetLenght - targetArray.Length < _size)
            {
                throw new ArgumentException();
            }
            List<T> list = new List<T>();
            foreach (T item in this)
            {
                list.Add(item);
            }
            targetArray = list.ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool Contains(BinaryTreeNode<T> current, T item)
        {
            if (current == null)
            {
                return false;
            }
            if (item.CompareTo(current.Value) == 0)
            {
                return true;
            }
            return item.CompareTo(current.Value) < 0 ? Contains(current.Left, item) : Contains(current.Right, item);
        }

        private void InOrderTraversal(BinaryTreeNode<T> node, Action<T> action)
        {
            if (node == null) return;

            InOrderTraversal(node.Left, action);
            action(node.Value);
            InOrderTraversal(node.Right, action);
        }

        private T Min(BinaryTreeNode<T> current)
        {
            if (current == null)
            {
                throw new ArgumentNullException("Node is not found!");
            }
            if (current.Left == null)
            {
                return current.Value;
            }
            return Min(current.Left);
        }

        private T Max(BinaryTreeNode<T> current)
        {
            if (current == null)
            {
                throw new ArgumentNullException("Node is not found!");
            }
            if (current.Right == null)
            {
                return current.Value;
            }
            return Max(current.Right);
        }

        private struct Enumerator : IEnumerator<T>
        {
            private Stack<BinaryTreeNode<T>> _stack;
            private BinaryTreeNode<T> _current;
            private int _size;

            public Enumerator(BinaryTreeNode<T> root, int size)
            {
                _size = size;
                _stack = new Stack<BinaryTreeNode<T>>(_size);
                _current = root;
                PushNode(_current);
            }

            private void PushNode(BinaryTreeNode<T> node)
            {
                while (node != null)
                {
                    _stack.Push(node);
                    node = node.Left;
                }
            }

            public T Current => _stack.Peek().Value;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_stack.Count == 0)
                {
                    return false;
                }

                _current = _stack.Pop();
                PushNode(_current.Right);
                return true;
            }

            public void Reset()
            {
                _stack.Clear();
                _current = null;
                PushNode(_current);
            }

            public void Dispose()
            {
                _current = default;
            }
        }
    }
}