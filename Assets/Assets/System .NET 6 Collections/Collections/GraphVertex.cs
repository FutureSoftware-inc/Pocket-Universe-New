using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Internal representation of a vertex to hold adjacencies inside a dense flat array layout.
/// </summary>
internal struct GraphVertex<TElement, TWeight>
{
    // Плоский массив ребер для экономии памяти вместо прожорливого List
    internal GraphEdge<TElement, TWeight>[] _edges;
    internal int _edgeCount;

    internal void AddEdge(TElement target, TWeight weight)
    {
        if (_edges == null)
        {
            _edges = new GraphEdge<TElement, TWeight>[Constants.MinimumGrow];
            _edges[0] = new GraphEdge<TElement, TWeight>(target, weight);
            _edgeCount = 1;
            return;
        }

        if (_edgeCount == _edges.Length)
        {
            int newCapacity = _edges.Length * Constants.GrowFactor;
            if ((uint)newCapacity > Constants.MaxLength) newCapacity = Constants.MaxLength;
            newCapacity = Math.Max(newCapacity, _edges.Length + Constants.MinimumGrow);

            Array.Resize(ref _edges, newCapacity);
        }

        _edges[_edgeCount++] = new GraphEdge<TElement, TWeight>(target, weight);
    }

    internal bool RemoveEdge(TElement target, IEqualityComparer<TElement> comparer)
    {
        if (_edges == null) return false;

        for (int i = 0; i < _edgeCount; i++)
        {
            if (comparer.Equals(_edges[i].Target, target))
            {
                _edgeCount--;
                if (i < _edgeCount)
                {
                    Array.Copy(_edges, i + 1, _edges, i, _edgeCount - i);
                }
                _edges[_edgeCount] = default;
                return true;
            }
        }

        return false;
    }

    // Добавьте этот метод внутрь структуры GraphVertex в первом файле:
    internal readonly int FindEdgeIndex(TElement target, IEqualityComparer<TElement> comparer)
    {
        if (_edges == null) return -1;
        for (int i = 0; i < _edgeCount; i++)
        {
            if (comparer.Equals(_edges[i].Target, target)) return i;
        }
        return -1;
    }

}