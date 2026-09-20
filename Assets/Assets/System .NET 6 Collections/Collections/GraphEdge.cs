namespace System.Collections.Generic
{
    /// <summary>
    /// Represents an edge (link) in a high-performance weighted graph.
    /// </summary>
    public struct GraphEdge<TElement, TWeight>
    {
        private readonly TElement _target;
        private readonly TWeight _weight;

        public GraphEdge(TElement target, TWeight weight)
        {
            _target = target;
            _weight = weight;
        }

        /// <summary>
        /// Gets the target element this edge points to.
        /// </summary>
        public TElement Target => _target;

        /// <summary>
        /// Gets the weight (cost) associated with this edge.
        /// </summary>
        public TWeight Weight => _weight;
    }
}