// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, probabilistic Counting Bloom Filter that supports element removal.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements to track (e.g., string for player nicknames).</typeparam>
    [DebuggerDisplay("Capacity = {Capacity}, ErrorRate = {ErrorRate}")]
    public class CountingBloomFilter<T>
    {
        // Вместо битов используем байты-счетчики (значения от 0 до 255)
        private readonly byte[] _counters;
        private readonly int _bitSize;
        private readonly int _hashFunctionsCount;
        private readonly IEqualityComparer<T> _comparer;
        private readonly int _capacity;
        private readonly double _errorRate;

        public CountingBloomFilter(int capacity, double errorRate = 0.01, IEqualityComparer<T> comparer = null)
        {
            capacity.ThrowIfNegative(nameof(capacity));
            if (capacity == 0) capacity = 1;
            if (errorRate <= 0.0 || errorRate >= 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(errorRate), "Error rate must be between 0 and 1 (exclusive).");
            }

            _capacity = capacity;
            _errorRate = errorRate;
            _comparer = comparer ?? EqualityComparer<T>.Default;

            // Математический расчет размера таблицы счетчиков
            double m = -((double)capacity * Math.Log(errorRate)) / Math.Pow(Math.Log(2.0), 2.0);
            _bitSize = (int)Math.Ceiling(m);

            // Математический расчет количества хэш-функций
            double k = ((double)_bitSize / (double)capacity) * Math.Log(2.0);
            _hashFunctionsCount = (int)Math.Ceiling(k);
            if (_hashFunctionsCount < 1) _hashFunctionsCount = 1;

            _counters = new byte[_bitSize];
        }

        public int Capacity => _capacity;
        public double ErrorRate => _errorRate;

        /// <summary>
        /// Adds an item to the filter by incrementing its corresponding counters.
        /// </summary>
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            int primaryHash = _comparer.GetHashCode(item);
            uint hashA = (uint)primaryHash;
            uint hashB = (uint)(primaryHash ^ (int)0x55555555);

            for (int i = 0; i < _hashFunctionsCount; i++)
            {
                uint combinedHash = hashA + ((uint)i * hashB);
                int index = (int)(combinedHash % (uint)_bitSize);

                // Защита от переполнения byte (максимум 255). 
                // Если счетчик достиг максимума, оставляем его 255 (навечно), чтобы не сломать логику.
                if (_counters[index] < byte.MaxValue)
                {
                    _counters[index]++;
                }
            }
        }

        /// <summary>
        /// Removes an item from the filter by decrementing its corresponding counters.
        /// </summary>
        /// <returns><see langword="true"/> if the item was likely in the filter and removed; <see langword="false"/> if it definitely wasn't there.</returns>
        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // Сначала проверяем, есть ли вообще смысл удалять
            if (!Contains(item))
            {
                return false;
            }

            int primaryHash = _comparer.GetHashCode(item);
            uint hashA = (uint)primaryHash;
            uint hashB = (uint)(primaryHash ^ (int)0x55555555);

            for (int i = 0; i < _hashFunctionsCount; i++)
            {
                uint combinedHash = hashA + ((uint)i * hashB);
                int index = (int)(combinedHash % (uint)_bitSize);

                // Уменьшаем счетчик только если он не зафиксирован на максимуме и больше 0
                if (_counters[index] > 0 && _counters[index] < byte.MaxValue)
                {
                    _counters[index]--;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if an item is likely present in the filter.
        /// </summary>
        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            int primaryHash = _comparer.GetHashCode(item);
            uint hashA = (uint)primaryHash;
            uint hashB = (uint)(primaryHash ^ (int)0x55555555);

            for (int i = 0; i < _hashFunctionsCount; i++)
            {
                uint combinedHash = hashA + ((uint)i * hashB);
                int index = (int)(combinedHash % (uint)_bitSize);

                // Если хотя бы один счетчик равен 0 — элемента точно нет в базе
                if (_counters[index] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the filter by clearing all counters.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_counters, 0, _counters.Length);
        }
    }
}
