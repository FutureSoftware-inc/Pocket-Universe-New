// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, generic weighted graph structure.
    /// </summary>
    /// <typeparam name="TElement">Specifies the type of data inside the graph vertices.</typeparam>
    /// <typeparam name="TWeight">Specifies the type of weight (cost) associated with edges.</typeparam>
    [DebuggerDisplay("VerticesCount = {VerticesCount}, IsDirected = {IsDirected}")]
    public class Graph<TElement, TWeight> : IEnumerable<TElement> where TElement : notnull where TWeight : IComparable<TWeight>, IConvertible
    {
        private readonly Dictionary<TElement, GraphVertex<TElement, TWeight>> _vertices;
        private readonly bool _isDirected;
        private readonly IEqualityComparer<TElement> _elementComparer;
        private int _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph{TElement, TWeight}"/> class.
        /// </summary>
        /// <param name="isDirected">Determines if the graph is directed (digraph). Default is false.</param>
        /// <param name="comparer">Custom equality comparer for elements, or null to use default.</param>
        public Graph(bool isDirected = false, IEqualityComparer<TElement> comparer = null)
        {
            _isDirected = isDirected;
            _elementComparer = comparer ?? EqualityComparer<TElement>.Default;
            _vertices = new Dictionary<TElement, GraphVertex<TElement, TWeight>>(_elementComparer);
        }

        /// <summary>
        /// Gets the total number of vertices inside the graph.
        /// </summary>
        public int VerticesCount => _vertices.Count;

        /// <summary>
        /// Gets a value indicating whether the graph is directed.
        /// </summary>
        public bool IsDirected => _isDirected;

        /// <summary>
        /// Adds a new vertex containing the specified element to the graph.
        /// </summary>
        /// <returns><see langword="true"/> if the vertex was successfully added; <see langword="false"/> if it already exists.</returns>
        public bool AddVertex(TElement element)
        {
            element.ThrowIfNull(nameof(element));

            if (_vertices.ContainsKey(element))
            {
                return false;
            }

            _vertices.Add(element, new GraphVertex<TElement, TWeight>());
            _version++;
            return true;
        }

        /// <summary>
        /// Adds or updates a weighted edge between two vertices. 
        /// Automatically creates vertices if they do not exist in the graph.
        /// </summary>
        public void AddEdge(TElement source, TElement target, TWeight weight)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            // Гарантируем наличие обеих вершин в графе
            AddVertex(source);
            AddVertex(target);

            // Получаем структуру по ссылке (через CollectionsMarshal в .NET, но для совместимости с Unity используем стандартный Dictionary-путь)
            GraphVertex<TElement, TWeight> sourceVertex = _vertices[source];
            sourceVertex.AddEdge(target, weight);
            _vertices[source] = sourceVertex; // Обновляем структуру в Dictionary

            // Если граф ненаправленный — зеркально добавляем обратную связь
            if (!_isDirected)
            {
                GraphVertex<TElement, TWeight> targetVertex = _vertices[target];
                targetVertex.AddEdge(source, weight);
                _vertices[target] = targetVertex;
            }

            _version++;
        }

        /// <summary>
        /// Removes an edge between two vertices.
        /// </summary>
        /// <returns><see langword="true"/> if the edge was successfully removed; otherwise, <see langword="false"/>.</returns>
        public bool RemoveEdge(TElement source, TElement target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            if (!_vertices.TryGetValue(source, out GraphVertex<TElement, TWeight> sourceVertex))
            {
                return false;
            }

            bool removed = sourceVertex.RemoveEdge(target, _elementComparer);
            if (removed)
            {
                _vertices[source] = sourceVertex; // Записываем обновленную структуру назад

                // Если граф ненаправленный, удаляем и зеркальное ребро
                if (!_isDirected && _vertices.TryGetValue(target, out GraphVertex<TElement, TWeight> targetVertex))
                {
                    targetVertex.RemoveEdge(source, _elementComparer);
                    _vertices[target] = targetVertex;
                }

                _version++;
            }

            return removed;
        }

        /// <summary>
        /// Removes a vertex and all its connected edges from the graph.
        /// </summary>
        /// <returns><see langword="true"/> if the vertex was successfully removed; otherwise, <see langword="false"/>.</returns>
        public bool RemoveVertex(TElement element)
        {
            element.ThrowIfNull(nameof(element));

            if (!_vertices.TryGetValue(element, out GraphVertex<TElement, TWeight> targetVertex))
            {
                return false;
            }

            // Если граф ненаправленный, нужно зайти к каждому соседу этой вершины
            // и удалить упоминание о ней из их локальных массивов ребер
            if (!_isDirected && targetVertex._edges != null)
            {
                for (int i = 0; i < targetVertex._edgeCount; i++)
                {
                    TElement neighbor = targetVertex._edges[i].Target;
                    if (_vertices.TryGetValue(neighbor, out GraphVertex<TElement, TWeight> neighborVertex))
                    {
                        neighborVertex.RemoveEdge(element, _elementComparer);
                        _vertices[neighbor] = neighborVertex;
                    }
                }
            }
            else if (_isDirected)
            {
                // В направленном графе (диграфе) придется сделать полный обход всех вершин,
                // чтобы вычистить входящие ребра, указывающие на удаляемый элемент
                foreach (KeyValuePair<TElement, GraphVertex<TElement, TWeight>> kvp in _vertices)
                {
                    GraphVertex<TElement, TWeight> vertex = kvp.Value;
                    if (vertex.RemoveEdge(element, _elementComparer))
                    {
                        _vertices[kvp.Key] = vertex;
                    }
                }
            }

            _vertices.Remove(element);
            _version++;
            return true;
        }

        /// <summary>
        /// Returns an enumerable range that traverses the graph using Breadth-First Search (BFS) starting from the specified vertex.
        /// </summary>
        public BfsRange TraverseBreadthFirst(TElement startVertex)
        {
            startVertex.ThrowIfNull(nameof(startVertex));
            if (!_vertices.ContainsKey(startVertex))
            {
                throw new KeyNotFoundException($"Start vertex '{startVertex}' does not exist in the graph.");
            }
            return new BfsRange(this, startVertex);
        }

        /// <summary>
        /// Returns an enumerable range that traverses the graph using Depth-First Search (DFS) starting from the specified vertex.
        /// </summary>
        public DfsRange TraverseDepthFirst(TElement startVertex)
        {
            startVertex.ThrowIfNull(nameof(startVertex));
            if (!_vertices.ContainsKey(startVertex))
            {
                throw new KeyNotFoundException($"Start vertex '{startVertex}' does not exist in the graph.");
            }
            return new DfsRange(this, startVertex);
        }

        // Измените ограничение в сигнатуре класса на IComparable и IConvertible:
        // public class Graph<TElement, TWeight> : IEnumerable<TElement> 
        //     where TElement : notnull 
        //     where TWeight : IComparable<TWeight>, IConvertible

        /// <summary>
        /// Finds the shortest path between source and target vertices using Dijkstra's algorithm.
        /// Compatible with older C# / Unity versions.
        /// </summary>
        public IReadOnlyList<TElement> FindShortestPath(TElement source, TElement target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            if (!_vertices.ContainsKey(source) || !_vertices.ContainsKey(target))
            {
                return Array.Empty<TElement>();
            }

            // Используем double для внутренних математических расчетов расстояний
            var distances = new Dictionary<TElement, double>(_elementComparer);
            var previous = new Dictionary<TElement, TElement>(_elementComparer);

            // Наша PriorityQueue для быстрого извлечения узла с минимальным весом
            var priorityQueue = new PriorityQueue<TElement, double>();

            distances[source] = 0.0;
            priorityQueue.Enqueue(source, 0.0);

            while (priorityQueue.Count > 0)
            {
                TElement current = priorityQueue.Dequeue();

                if (_elementComparer.Equals(current, target))
                {
                    break;
                }

                if (!_vertices.TryGetValue(current, out GraphVertex<TElement, TWeight> vertex) || vertex._edges == null)
                {
                    continue;
                }

                double currentDistance = distances[current];

                for (int i = 0; i < vertex._edgeCount; i++)
                {
                    GraphEdge<TElement, TWeight> edge = vertex._edges[i];
                    TElement neighbor = edge.Target;

                    // Безопасная конвертация веса любого типа (int, float, double) в double через IConvertible
                    double edgeWeight = edge.Weight.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
                    double newDistance = currentDistance + edgeWeight;

                    if (!distances.TryGetValue(neighbor, out double oldDistance) || newDistance < oldDistance)
                    {
                        distances[neighbor] = newDistance;
                        previous[neighbor] = current;
                        priorityQueue.Enqueue(neighbor, newDistance);
                    }
                }
            }

            if (!previous.ContainsKey(target) && !_elementComparer.Equals(source, target))
            {
                return Array.Empty<TElement>();
            }

            var path = new Deque<TElement>();
            TElement currPathNode = target;

            // Восстанавливаем путь с конца в начало
            while (currPathNode != null)
            {
                path.AddFirst(currPathNode);
                if (_elementComparer.Equals(currPathNode, source)) break;

                currPathNode = previous.TryGetValue(currPathNode, out TElement prev) ? prev : default!;
            }

            return path.ToArray();
        }


        public readonly struct BfsRange : IEnumerable<TElement>
        {
            private readonly Graph<TElement, TWeight> _graph;
            private readonly TElement _start;

            internal BfsRange(Graph<TElement, TWeight> graph, TElement start)
            {
                _graph = graph;
                _start = start;
            }

            public BfsEnumerator GetEnumerator() => new BfsEnumerator(_graph, _start);
            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        /// <summary>
        /// Removes all vertices and edges from the graph.
        /// </summary>
        public void Clear()
        {
            _vertices.Clear();
            _version++;
        }

        /// <summary>
        /// Determines whether an edge exists between two vertices.
        /// </summary>
        public bool HasEdge(TElement source, TElement target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            if (_vertices.TryGetValue(source, out GraphVertex<TElement, TWeight> vertex))
            {
                return vertex.FindEdgeIndex(target, _elementComparer) >= 0;
            }

            return false;
        }

        /// <summary>
        /// Tries to get the weight of the edge between two vertices.
        /// </summary>
        public bool TryGetEdgeWeight(TElement source, TElement target, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TWeight weight)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            if (_vertices.TryGetValue(source, out GraphVertex<TElement, TWeight> vertex))
            {
                int index = vertex.FindEdgeIndex(target, _elementComparer);
                if (index >= 0)
                {
                    weight = vertex._edges[index].Weight;
                    return true;
                }
            }

            weight = default;
            return false;
        }

        /// <summary>
        /// Returns a zero-allocation read-only range of edges originating from the specified vertex.
        /// </summary>
        public ReadOnlyEdgesRange GetEdges(TElement vertexElement)
        {
            vertexElement.ThrowIfNull(nameof(vertexElement));

            if (!_vertices.TryGetValue(vertexElement, out GraphVertex<TElement, TWeight> vertex))
            {
                throw new KeyNotFoundException($"Vertex '{vertexElement}' does not exist in the graph.");
            }

            return new ReadOnlyEdgesRange(vertex._edges, vertex._edgeCount);
        }

        /// <summary>
        /// Infrastructure structure to expose vertex edges without any heap allocations.
        /// </summary>
        public readonly struct ReadOnlyEdgesRange : IEnumerable<GraphEdge<TElement, TWeight>>
        {
            private readonly GraphEdge<TElement, TWeight>[] _edges;
            private readonly int _count;

            internal ReadOnlyEdgesRange(GraphEdge<TElement, TWeight>[] edges, int count)
            {
                _edges = edges;
                _count = count;
            }

            public int Count => _count;

            public Enumerator GetEnumerator() => new Enumerator(_edges, _count);

            IEnumerator<GraphEdge<TElement, TWeight>> IEnumerable<GraphEdge<TElement, TWeight>>.GetEnumerator() =>
                _count == 0 ? EnumerableHelpers.GetEmptyEnumerator<GraphEdge<TElement, TWeight>>() : GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<GraphEdge<TElement, TWeight>>)this).GetEnumerator();

            public struct Enumerator : IEnumerator<GraphEdge<TElement, TWeight>>
            {
                private readonly GraphEdge<TElement, TWeight>[] _edges;
                private readonly int _count;
                private int _index;
                private GraphEdge<TElement, TWeight> _current;

                internal Enumerator(GraphEdge<TElement, TWeight>[] edges, int count)
                {
                    _edges = edges;
                    _count = count;
                    _index = 0;
                    _current = default;
                }

                public bool MoveNext()
                {
                    if (_edges != null && (uint)_index < (uint)_count)
                    {
                        _current = _edges[_index++];
                        return true;
                    }
                    _index = _count + 1;
                    _current = default;
                    return false;
                }

                public GraphEdge<TElement, TWeight> Current => _current;

                object IEnumerator.Current => _current;

                public void Dispose() { }

                void IEnumerator.Reset()
                {
                    _index = 0;
                    _current = default;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through all vertices in the graph.
        /// </summary>
        public VertexEnumerator GetEnumerator() => new VertexEnumerator(this);

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() =>
            _vertices.Count == 0 ? EnumerableHelpers.GetEmptyEnumerator<TElement>() : GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TElement>)this).GetEnumerator();

        /// <summary>
        /// Enumerates the unique element identifiers (vertices) of a <see cref="Graph{TElement, TWeight}"/>.
        /// </summary>
        public struct VertexEnumerator : IEnumerator<TElement>
        {
            private readonly Graph<TElement, TWeight> _graph;
            private readonly int _version;
            private Dictionary<TElement, GraphVertex<TElement, TWeight>>.KeyCollection.Enumerator _innerEnumerator;
            private TElement _current;

            internal VertexEnumerator(Graph<TElement, TWeight> graph)
            {
                _graph = graph;
                _version = graph._version;
                _innerEnumerator = graph._vertices.Keys.GetEnumerator();
                _current = default!;
            }

            public bool MoveNext()
            {
                if (_version != _graph._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                if (_innerEnumerator.MoveNext())
                {
                    _current = _innerEnumerator.Current;
                    return true;
                }

                _current = default!;
                return false;
            }

            public TElement Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
                _innerEnumerator.Dispose();
            }

            void IEnumerator.Reset()
            {
                if (_version != _graph._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                // Переинициализируем внутренний итератор ключей Dictionary
                _innerEnumerator = _graph._vertices.Keys.GetEnumerator();
                _current = default!;
            }
        }

        public struct BfsEnumerator : IEnumerator<TElement>
        {
            private readonly Graph<TElement, TWeight> _graph;
            private readonly int _version;
            private Deque<TElement> _queue;
            private HashSet<TElement> _visited;
            private TElement _current;
            private bool _isStarted;

            internal BfsEnumerator(Graph<TElement, TWeight> graph, TElement start)
            {
                _graph = graph;
                _version = graph._version;
                _queue = new Deque<TElement>();
                _visited = new HashSet<TElement>(graph._elementComparer);
                _current = start;
                _isStarted = false;

                _queue.AddLast(start);
                _visited.Add(start);
            }

            public bool MoveNext()
            {
                if (_version != _graph._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                if (_queue.Count == 0)
                {
                    _current = default!;
                    return false;
                }

                _current = _queue.RemoveFirst();

                if (_graph._vertices.TryGetValue(_current, out GraphVertex<TElement, TWeight> vertex) && vertex._edges != null)
                {
                    for (int i = 0; i < vertex._edgeCount; i++)
                    {
                        TElement neighbor = vertex._edges[i].Target;
                        if (_visited.Add(neighbor))
                        {
                            _queue.AddLast(neighbor);
                        }
                    }
                }

                return true;
            }

            public TElement Current => _current;
            object IEnumerator.Current => _current;
            public void Dispose() { }
            void IEnumerator.Reset() => throw new NotSupportedException();
        }

        public readonly struct DfsRange : IEnumerable<TElement>
        {
            private readonly Graph<TElement, TWeight> _graph;
            private readonly TElement _start;

            internal DfsRange(Graph<TElement, TWeight> graph, TElement start)
            {
                _graph = graph;
                _start = start;
            }

            public DfsEnumerator GetEnumerator() => new DfsEnumerator(_graph, _start);
            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct DfsEnumerator : IEnumerator<TElement>
        {
            private readonly Graph<TElement, TWeight> _graph;
            private readonly int _version;
            private Stack<TElement> _stack;
            private HashSet<TElement> _visited;
            private TElement _current;

            internal DfsEnumerator(Graph<TElement, TWeight> graph, TElement start)
            {
                _graph = graph;
                _version = graph._version;
                _stack = new Stack<TElement>();
                _visited = new HashSet<TElement>(graph._elementComparer);
                _current = default!;

                _stack.Push(start);
            }

            public bool MoveNext()
            {
                if (_version != _graph._version)
                {
                    throw new InvalidOperationException(ErrorMessage.InvalidOperation.EnumFailedVersion);
                }

                while (_stack.Count > 0)
                {
                    TElement element = _stack.Pop();

                    if (_visited.Add(element))
                    {
                        _current = element;

                        if (_graph._vertices.TryGetValue(_current, out GraphVertex<TElement, TWeight> vertex) && vertex._edges != null)
                        {
                            // Кладём соседей в стек в обратном порядке для естественного DFS
                            for (int i = vertex._edgeCount - 1; i >= 0; i--)
                            {
                                TElement neighbor = vertex._edges[i].Target;
                                if (!_visited.Contains(neighbor))
                                {
                                    _stack.Push(neighbor);
                                }
                            }
                        }
                        return true;
                    }
                }

                _current = default!;
                return false;
            }

            public TElement Current => _current;
            object IEnumerator.Current => _current;
            public void Dispose() { }
            void IEnumerator.Reset() => throw new NotSupportedException();
        }

    }
}

