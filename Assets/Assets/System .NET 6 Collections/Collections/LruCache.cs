// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, array-backed Least Recently Used (LRU) Cache 
    /// with O(1) lookups and zero per-node heap allocations.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
    public class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        private readonly Dictionary<TKey, int> _indexMap;
        private LruNode<TKey, TValue>[] _nodes;

        private readonly int _capacity;
        private int _head;     // Индекс самого свежего элемента (Голова списка)
        private int _tail;     // Индекс самого старого элемента (Хвост списка)
        private int _freeHead; // Индекс начала списка свободных ячеек в массиве
        private int _count;
        private int _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="LruCache{TKey, TValue}"/> class with a fixed capacity.
        /// </summary>
        public LruCache(int capacity, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }

            _capacity = capacity;
            _indexMap = new Dictionary<TKey, int>(capacity, comparer ?? EqualityComparer<TKey>.Default);

            // Выделяем плоский массив под узлы ровно под заданную емкость
            _nodes = new LruNode<TKey, TValue>[capacity];

            _head = -1;
            _tail = -1;

            // Изначально связываем все ячейки массива в цепочку свободных элементов
            for (int i = 0; i < capacity - 1; i++)
            {
                _nodes[i]._next = i + 1;
            }
            _nodes[capacity - 1]._next = -1;
            _freeHead = 0;
        }

        public int Count => _count;
        public int Capacity => _capacity;

        /// <summary>
        /// Moves an existing node in the flat array to the front (head) of the list.
        /// </summary>
        private void MoveToHead(int index)
        {
            if (_head == index) return;

            // Отсоединяем узел из его текущей позиции
            int prev = _nodes[index]._previous;
            int next = _nodes[index]._next;

            if (prev != -1) _nodes[prev]._next = next;
            if (next != -1) _nodes[next]._previous = prev;

            if (_tail == index) _tail = prev;

            // Прикрепляем узел в голову списка
            _nodes[index]._next = _head;
            _nodes[index]._previous = -1;

            if (_head != -1) _nodes[_head]._previous = index;
            _head = index;

            if (_tail == -1) _tail = index;
        }

        /// <summary>
        /// Removes a node from the doubly linked layout.
        /// </summary>
        private void RemoveFromList(int index)
        {
            int prev = _nodes[index]._previous;
            int next = _nodes[index]._next;

            if (prev != -1) _nodes[prev]._next = next;
            if (next != -1) _nodes[next]._previous = prev;

            if (_head == index) _head = next;
            if (_tail == index) _tail = prev;
        }

        /// <summary>
        /// Adds a key-value pair to the cache, or updates the value if the key already exists.
        /// If the cache is full, evicts the least recently used item.
        /// </summary>
        public void AddOrUpdate(TKey key, TValue value)
        {
            key.ThrowIfNull(nameof(key));

            _version++;

            // Сценарий 1: Элемент уже есть в кэше — просто обновляем значение и двигаем в начало
            if (_indexMap.TryGetValue(key, out int existingIndex))
            {
                _nodes[existingIndex]._value = value;
                MoveToHead(existingIndex);
                return;
            }

            int targetIndex;

            // Сценарий 2: Кэш полностью забит — вытесняем (Evict) самый старый элемент из хвоста
            if (_count >= _capacity)
            {
                targetIndex = _tail;
                Debug.Assert(targetIndex != -1);

                // Удаляем старый ключ из хэш-карты
                _indexMap.Remove(_nodes[targetIndex]._key);

                // Отсоединяем хвост от списка
                RemoveFromList(targetIndex);

                // Зануляем ссылки для GC, если тип ссылочный
                _nodes[targetIndex].Invalidate();
            }
            // Сценарий 3: Есть свободное место в массиве — берем ячейку из пула свободных
            else
            {
                targetIndex = _freeHead;
                Debug.Assert(targetIndex != -1);

                _freeHead = _nodes[targetIndex]._next;
                _count++;
            }

            // Инициализируем ячейку новыми данными
            _nodes[targetIndex].Initialize(key, value);
            _indexMap.Add(key, targetIndex);

            // Помещаем новую ячейку в голову списка
            _nodes[targetIndex]._next = _head;
            if (_head != -1)
            {
                _nodes[_head]._previous = targetIndex;
            }
            _head = targetIndex;

            if (_tail == -1)
            {
                _tail = targetIndex;
            }
        }

        /// <summary>
        /// Tries to get the value associated with the specified key. 
        /// Automatically marks the item as recently used if found.
        /// </summary>
        public bool TryGetValue(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
        {
            key.ThrowIfNull(nameof(key));

            if (_indexMap.TryGetValue(key, out int index))
            {
                value = _nodes[index]._value;

                // Важнейший шаг LRU: так как к элементу обратились, двигаем его в голову
                MoveToHead(index);
                _version++;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Checks if the cache contains the specified key without modifying its LRU position.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            key.ThrowIfNull(nameof(key));
            return _indexMap.ContainsKey(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the cache.
        /// </summary>
        /// <returns><see langword="true"/> if the element was successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(TKey key)
        {
            key.ThrowIfNull(nameof(key));

            if (!_indexMap.TryGetValue(key, out int index))
            {
                return false;
            }

            _version++;

            // Удаляем ключ из хэш-карты и узел из двусвязного списка
            _indexMap.Remove(key);
            RemoveFromList(index);

            // Очищаем данные узла
            _nodes[index].Invalidate();

            // Возвращаем освободившуюся ячейку обратно в пул свободных мест
            _nodes[index]._next = _freeHead;
            _freeHead = index;

            _count--;
            return true;
        }

        /// <summary>
        /// Clears all elements from the cache and resets internal structures.
        /// </summary>
        public void Clear()
        {
            _indexMap.Clear();
            _head = -1;
            _tail = -1;
            _count = 0;
            _version++;

            // Пересобираем пул свободных ячеек в массиве
            int capacity = _capacity;
            for (int i = 0; i < capacity - 1; i++)
            {
                _nodes[i].Invalidate();
                _nodes[i]._next = i + 1;
            }
            _nodes[capacity - 1].Invalidate();
            _nodes[capacity - 1]._next = -1;
            _freeHead = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the cache from most-recently to least-recently used.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            _count == 0 ? EnumerableHelpers.GetEmptyEnumerator<KeyValuePair<TKey, TValue>>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();

        /// <summary>
        /// Enumerates the elements of an <see cref="LruCache{TKey, TValue}"/> from newest to oldest.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly LruCache<TKey, TValue> _cache;
            private readonly int _version;
            private int _currentIndex;
            private KeyValuePair<TKey, TValue> _current;
            private bool _isStarted;

            internal Enumerator(LruCache<TKey, TValue> cache)
            {
                _cache = cache;
                _version = cache._version;
                _currentIndex = cache._head;
                _current = default;
                _isStarted = false;
            }

            public bool MoveNext()
            {
                if (_version != _cache._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                if (!_isStarted)
                {
                    _isStarted = true;
                    _currentIndex = _cache._head;
                }

                if (_currentIndex == -1)
                {
                    _current = default;
                    return false;
                }

                LruNode<TKey, TValue> node = _cache._nodes[_currentIndex];
                _current = new KeyValuePair<TKey, TValue>(node._key, node._value);

                // Переходим к следующему по старости элементу списка
                _currentIndex = node._next;
                return true;
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { }

            void IEnumerator.Reset()
            {
                if (_version != _cache._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                _currentIndex = _cache._head;
                _current = default;
                _isStarted = false;
            }
        }
    }
}