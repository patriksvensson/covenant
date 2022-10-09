namespace Covenant;

[DebuggerDisplay("{Start,nq} => {End,nq}")]
public sealed class GraphEdge<T>
{
    public T Start { get; }
    public T End { get; }
    public string? Metadata { get; init; }

    public GraphEdge(T start, T end)
    {
        Start = start;
        End = end;
    }
}
