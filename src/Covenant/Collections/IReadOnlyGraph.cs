namespace Covenant;

public interface IReadOnlyGraph<T>
{
    public IReadOnlySet<T> Nodes { get; }
    public IReadOnlySet<GraphEdge<T>> Edges { get; }

    bool Exist(T item);
    IEnumerable<T> GetOutgoingNodes(T node);
}
