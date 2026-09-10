using System.Collections.Generic;

/// <summary>
/// Proxy view for the debugger to easily inspect tree hierarchy.
/// </summary>
internal sealed class TreeDebugView<T>
{
    private readonly Tree<T> _tree;

    public TreeDebugView(Tree<T> tree)
    {
        tree.ThrowIfNull(nameof(tree));
        _tree = tree;
    }

    public TreeNode<T> Root => _tree.Root;
    public int TotalNodes => _tree.Count;
}