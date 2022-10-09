namespace Covenant;

public sealed class Graph<T> : IReadOnlyGraph<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private readonly HashSet<T> _nodes;
    private readonly HashSet<GraphEdge<T>> _edges;

    public ISet<T> Nodes => _nodes;
    public ISet<GraphEdge<T>> Edges => _edges;

    IReadOnlySet<T> IReadOnlyGraph<T>.Nodes => _nodes;
    IReadOnlySet<GraphEdge<T>> IReadOnlyGraph<T>.Edges => _edges;

    public Graph(IEqualityComparer<T>? comparer)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        _nodes = new HashSet<T>(_comparer);
        _edges = new HashSet<GraphEdge<T>>();
    }

    public T Add(T node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var existing = _nodes.FirstOrDefault(x => _comparer.Equals(x, node));
        if (existing != null)
        {
            return existing;
        }

        _nodes.Add(node);
        return node;
    }

    public void Connect(T start, T end, string? metadata = null)
    {
        if (_comparer.Equals(start, end))
        {
            throw new InvalidOperationException("Reflexive edges in graph are not allowed.");
        }

        if (_edges.Any(x => _comparer.Equals(x.Start, start) && _comparer.Equals(x.End, end)))
        {
            return;
        }

        if (_nodes.All(x => !_comparer.Equals(x, start)))
        {
            _nodes.Add(start);
        }

        if (_nodes.All(x => !_comparer.Equals(x, end)))
        {
            _nodes.Add(end);
        }

        _edges.Add(new GraphEdge<T>(start, end)
        {
            Metadata = metadata,
        });
    }

    public bool Exist(T item)
    {
        return _nodes.Any(x => _comparer.Equals(x, item));
    }

    public IEnumerable<T> GetOutgoingNodes(T node)
    {
        return _edges
            .Where(e => _comparer.Equals(e.Start, node))
            .Select(x => x.End);
    }
}