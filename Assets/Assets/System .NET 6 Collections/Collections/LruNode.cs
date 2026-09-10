// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// Internal value type representing a node in a flat array-backed doubly linked list for LRU Cache.
    /// </summary>
    internal struct LruNode<TKey, TValue>
    {
        internal TKey _key;
        internal TValue _value;

        // Вместо ссылок используем индексы в массиве для идеальной локальности памяти
        internal int _previous;
        internal int _next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Initialize(TKey key, TValue value)
        {
            _key = key;
            _value = value;
            _previous = -1;
            _next = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Invalidate()
        {
            _key = default!;
            _value = default!;
            _previous = -1;
            _next = -1;
        }
    }
}
