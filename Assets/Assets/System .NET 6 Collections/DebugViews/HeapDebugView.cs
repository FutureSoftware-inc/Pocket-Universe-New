using System.Collections.Generic;

using System.Diagnostics;

/// <summary>
/// Proxy view for the debugger to easily inspect heap elements.
/// </summary>
internal sealed class HeapDebugView<T>
{
    private readonly Heap<T> _heap;

    public HeapDebugView(Heap<T> heap)
    {
        heap.ThrowIfNull(nameof(heap));
        _heap = heap;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _heap.ToArray();
}
