// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace System.Collections.Generic
{
    internal sealed class DequeDebugView<T>
    {
        private readonly Deque<T> _deque;

        public DequeDebugView(Deque<T> deque)
        {
            deque.ThrowIfNull(nameof(deque));
            _deque = deque;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _deque.ToArray();
    }
}
