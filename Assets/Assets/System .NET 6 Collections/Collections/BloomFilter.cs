// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, zero-allocation, probabilistic Bloom Filter.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements to track.</typeparam>
    [DebuggerDisplay("Capacity = {Capacity}, ErrorRate = {ErrorRate}")]
    public class BloomFilter<T>
    {
        // Наш кастомный ультра-быстрый битовый массив
        private readonly ulong[] _bitData;
        private readonly int _bitSize;
        private readonly int _hashFunctionsCount;
        private readonly IEqualityComparer<T> _comparer;
        private readonly int _capacity;
        private readonly double _errorRate;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter{T}"/> class based on capacity and desired error rate.
        /// </summary>
        /// <param name="capacity">Expected maximum number of elements to add.</param>
        /// <param name="errorRate">Acceptable false positive rate (e.g., 0.01 for 1%).</param>
        /// <param name="comparer">Custom equality comparer for hashing, or null to use default.</param>
        public BloomFilter(int capacity, double errorRate = 0.01, IEqualityComparer<T> comparer = null)
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

            // Математический расчет оптимального размера фильтра Блума (в битах)
            // m = - (n * ln(p)) / (ln(2)^2)
            double m = -((double)capacity * Math.Log(errorRate)) / Math.Pow(Math.Log(2.0), 2.0);
            _bitSize = (int)Math.Ceiling(m);

            // Математический расчет оптимального количества хэш-функций
            // k = (m / n) * ln(2)
            double k = ((double)_bitSize / (double)capacity) * Math.Log(2.0);
            _hashFunctionsCount = (int)Math.Ceiling(k);
            if (_hashFunctionsCount < 1) _hashFunctionsCount = 1;

            // Выделяем плоский массив ulong (в одном ulong содержится 64 бита)
            int ulongSize = (_bitSize + 63) / 64;
            _bitData = new ulong[ulongSize];
        }

        /// <summary>
        /// Gets the expected element capacity configured for this filter.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Gets the target false positive probability rate.
        /// </summary>
        public double ErrorRate => _errorRate;

        /// <summary>
        /// Adds an item to the Bloom Filter.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            // Получаем первичный 32-битный хэш от компаратора
            int primaryHash = _comparer.GetHashCode(item);

            // Алгоритм Кирша-Митценмахера: создаем две базы для генерации k хэшей
            uint hashA = (uint)primaryHash;
            uint hashB = (uint)(primaryHash ^ (int)0x55555555); // Инвертируем маску для дифференциации

            for (int i = 0; i < _hashFunctionsCount; i++)
            {
                // Формула: (hashA + i * hashB) % bitSize
                uint combinedHash = hashA + ((uint)i * hashB);
                int bitIndex = (int)(combinedHash % (uint)_bitSize);

                // Вычисляем позицию внутри массива ulong
                // bitIndex / 64 эквивалентно bitIndex >> 6
                int ulongIndex = bitIndex >> 6;

                // bitIndex % 64 эквивалентно bitIndex & 63
                int bitPosition = bitIndex & 63;

                // Включаем бит на уровне регистров процессора
                _bitData[ulongIndex] |= (1UL << bitPosition);
            }
        }

        /// <summary>
        /// Checks if an item is likely present in the Bloom Filter.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// <see langword="false"/> if the item is definitely NOT in the filter; 
        /// <see langword="true"/> if the item is likely present (with a small probability of a false positive).
        /// </returns>
        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            int primaryHash = _comparer.GetHashCode(item);

            uint hashA = (uint)primaryHash;
            uint hashB = (uint)(primaryHash ^ (int)0x55555555);

            for (int i = 0; i < _hashFunctionsCount; i++)
            {
                uint combinedHash = hashA + ((uint)i * hashB);
                int bitIndex = (int)(combinedHash % (uint)_bitSize);

                int ulongIndex = bitIndex >> 6;
                int bitPosition = bitIndex & 63;

                // Проверяем, включен ли бит. Если хотя бы один бит равен 0 — элемента точно нет.
                if ((_bitData[ulongIndex] & (1UL << bitPosition)) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the Bloom Filter by clearing all bits to zero.
        /// </summary>
        public void Clear()
        {
            // Очищаем весь массив ulong за одно обращение к памяти
            Array.Clear(_bitData, 0, _bitData.Length);
        }

        /// <summary>
        /// Combines this filter with another Bloom Filter using a bitwise OR operation.
        /// Both filters must have the same bit size and hash functions count.
        /// </summary>
        public void UnionWith(BloomFilter<T> other)
        {
            other.ThrowIfNull(nameof(other));

            if (_bitSize != other._bitSize || _hashFunctionsCount != other._hashFunctionsCount)
            {
                throw new ArgumentException("Filters must have identical sizes and hash configurations to perform a Union.");
            }

            // Объединение множеств за наносекунды через побитовое ИЛИ
            for (int i = 0; i < _bitData.Length; i++)
            {
                _bitData[i] |= other._bitData[i];
            }
        }

        /// <summary>
        /// Intersects this filter with another Bloom Filter using a bitwise AND operation.
        /// Both filters must have the same bit size and hash functions count.
        /// </summary>
        public void IntersectWith(BloomFilter<T> other)
        {
            other.ThrowIfNull(nameof(other));

            if (_bitSize != other._bitSize || _hashFunctionsCount != other._hashFunctionsCount)
            {
                throw new ArgumentException("Filters must have identical sizes and hash configurations to perform an Intersection.");
            }

            // Пересечение множеств за наносекунды через побитовое И
            for (int i = 0; i < _bitData.Length; i++)
            {
                _bitData[i] &= other._bitData[i];
            }
        }
    }
}