using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Proxy view for the debugger to easily inspect BST elements in sorted order.
/// </summary>
internal sealed class BinarySearchTreeDebugView<T>
{
    private readonly BinarySearchTree<T> _tree;

    public BinarySearchTreeDebugView(BinarySearchTree<T> tree)
    {
        tree.ThrowIfNull(nameof(tree));
        _tree = tree;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _tree.ToArray();
}
