// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a single node inside the high-performance Trie structure.
    /// </summary>
    internal struct TrieNode<TValue>
    {
        // Параллельные массивы: символы отсортированы для бинарного поиска
        internal char[] _childrenChars;
        internal TrieNode<TValue>[] _childrenNodes;

        internal TValue _value;
        internal bool _hasValue;

        /// <summary>
        /// Tries to find a child node index associated with the specified character using Binary Search.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly int FindChildIndex(char c)
        {
            if (_childrenChars == null)
            {
                return -1;
            }

            return Array.BinarySearch(_childrenChars, c);
        }

        /// <summary>
        /// Adds a child character and returns the index of the newly created or existing node.
        /// </summary>
        internal int AddChild(char c)
        {
            if (_childrenChars == null)
            {
                _childrenChars = new char[Constants.MinimumGrow];
                _childrenNodes = new TrieNode<TValue>[Constants.MinimumGrow];
                _childrenChars[0] = c;
                _childrenNodes[0] = new TrieNode<TValue>();
                return 0;
            }

            int index = Array.BinarySearch(_childrenChars, c);
            if (index >= 0)
            {
                return index; // Узел уже существует
            }

            // Индекс вставки, если элемент не найден
            int insertIndex = ~index;
            int currentLength = _childrenChars.Length;

            // Если массивы заполнены — расширяем их по нашему стандарту Constants
            // Так как в структурах длина логическая может отличаться, 
            // мы считаем заполненность по наличию дефолтных элементов в конце или по кастомному трекингу.
            // Для упрощения и надежности проверим, занято ли последнее место:
            bool isFull = _childrenChars[currentLength - 1] != '\0';

            if (isFull)
            {
                int newCapacity = currentLength * Constants.GrowFactor;
                if ((uint)newCapacity > Constants.MaxLength) newCapacity = Constants.MaxLength;
                newCapacity = Math.Max(newCapacity, currentLength + Constants.MinimumGrow);

                Array.Resize(ref _childrenChars, newCapacity);
                Array.Resize(ref _childrenNodes, newCapacity);
            }

            // Считаем текущее количество реальных детей (до первого '\0' или до конца массива)
            int actualCount = 0;
            while (actualCount < _childrenChars.Length && _childrenChars[actualCount] != '\0')
            {
                actualCount++;
            }

            // Сдвигаем элементы вправо, чтобы освободить место для нового отсортированного символа
            if (insertIndex < actualCount)
            {
                Array.Copy(_childrenChars, insertIndex, _childrenChars, insertIndex + 1, actualCount - insertIndex);
                Array.Copy(_childrenNodes, insertIndex, _childrenNodes, insertIndex + 1, actualCount - insertIndex);
            }

            _childrenChars[insertIndex] = c;
            _childrenNodes[insertIndex] = new TrieNode<TValue>();
            return insertIndex;
        }
    }
}