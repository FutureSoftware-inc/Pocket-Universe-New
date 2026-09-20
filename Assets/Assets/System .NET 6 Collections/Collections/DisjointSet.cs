// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, generic Disjoint-Set (Union-Find) data structure 
    /// with path compression and rank optimization.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the sets.</typeparam>
    [DebuggerDisplay("SetsCount = {SetsCount}, TotalElements = {TotalElements}")]
    public class DisjointSet<T> where T : notnull
    {
        // Массив индексов родителей. Если _parent[i] == i, то i — это корень (лидер) группы
        private int[] _parent;
        // Массив рангов для балансировки деревьев при слиянии
        private int[] _rank;

        // Быстрое сопоставление элемента с внутренним числовым индексом массива
        private readonly Dictionary<T, int> _elementToId;
        private readonly IEqualityComparer<T> _comparer;

        private int _setsCount;
        private int _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisjointSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">Custom equality comparer for elements, or null to use default.</param>
        public DisjointSet(IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _elementToId = new Dictionary<T, int>(_comparer);
            _parent = Array.Empty<int>();
            _rank = Array.Empty<int>();
        }

        /// <summary>
        /// Gets the total number of independent sets (groups) right now.
        /// </summary>
        public int SetsCount => _setsCount;

        /// <summary>
        /// Gets the total number of registered elements inside the structure.
        /// </summary>
        public int TotalElements => _elementToId.Count;

        /// <summary>
        /// Creates a new singleton set containing the specified element.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>true if the element was successfully added; false if it already exists.</returns>
        public bool MakeSet(T element)
        {
            element.ThrowIfNull(nameof(element));

            if (_elementToId.ContainsKey(element))
            {
                return false;
            }

            int id = _elementToId.Count;

            // Если внутренние массивы заполнены — расширяем их по нашему стандарту
            if (id == _parent.Length)
            {
                int newCapacity = _parent.Length == 0 ? Constants.MinimumGrow : _parent.Length * Constants.GrowFactor;
                if ((uint)newCapacity > Constants.MaxLength) newCapacity = Constants.MaxLength;

                Array.Resize(ref _parent, newCapacity);
                Array.Resize(ref _rank, newCapacity);
            }

            _elementToId.Add(element, id);
            _parent[id] = id; // Изначально элемент сам себе родитель (корень)
            _rank[id] = 0;    // Начальный ранг дерева равен 0

            _setsCount++;
            _version++;
            return true;
        }

        /// <summary>
        /// Finds the root representative (leader ID) of the set containing the given element ID.
        /// Performs iterative path compression.
        /// </summary>
        private int FindId(int id)
        {
            int root = id;

            // Шаг 1: Ищем корень дерева (у корня parent[i] == i)
            while (root != _parent[root])
            {
                root = _parent[root];
            }

            // Шаг 2: Сжатие пути (Path Compression)
            // Итеративно перевешиваем всех пройденных предков напрямую к корню
            int current = id;
            while (current != root)
            {
                int next = _parent[current];
                _parent[current] = root;
                current = next;
            }

            return root;
        }

        /// <summary>
        /// Determines whether two elements belong to the same set.
        /// </summary>
        public bool IsInSameSet(T first, T second)
        {
            first.ThrowIfNull(nameof(first));
            second.ThrowIfNull(nameof(second));

            if (!_elementToId.TryGetValue(first, out int firstId) ||
                !_elementToId.TryGetValue(second, out int secondId))
            {
                return false;
            }

            return FindId(firstId) == FindId(secondId);
        }

        /// <summary>
        /// Merges the sets containing the two specified elements.
        /// Uses Union by Rank optimization.
        /// </summary>
        /// <returns>true if the sets were successfully merged; false if they already belonged to the same set or were missing.</returns>
        public bool Union(T first, T second)
        {
            first.ThrowIfNull(nameof(first));
            second.ThrowIfNull(nameof(second));

            if (!_elementToId.TryGetValue(first, out int firstId) ||
                !_elementToId.TryGetValue(second, out int secondId))
            {
                return false;
            }

            // Находим корни (лидеров) для обеих групп
            int rootFirst = FindId(firstId);
            int rootSecond = FindId(secondId);

            // Если они уже в одном множестве, ничего объединять не нужно
            if (rootFirst == rootSecond)
            {
                return false;
            }

            // Оптимизация Union by Rank: вешаем меньшее дерево под большее
            if (_rank[rootFirst] < _rank[rootSecond])
            {
                _parent[rootFirst] = rootSecond;
            }
            else if (_rank[rootFirst] > _rank[rootSecond])
            {
                _parent[rootSecond] = rootFirst;
            }
            else
            {
                // Если ранги равны, делаем одно дерево родителем, а его ранг увеличиваем
                _parent[rootSecond] = rootFirst;
                _rank[rootFirst]++;
            }

            _setsCount--;
            _version++;
            return true;
        }

        /// <summary>
        /// Removes all elements and completely resets the Disjoint-Set structure.
        /// </summary>
        public void Clear()
        {
            _elementToId.Clear();
            _setsCount = 0;
            _version++;

            // Сбрасываем массивы, чтобы освободить память, если коллекция была огромной
            _parent = Array.Empty<int>();
            _rank = Array.Empty<int>();
        }
    }
}
