namespace Covenant;

public static class BomGraphExtensions
{
    public static BomComponent? FindByBomRef(this IReadOnlyGraph<BomComponent> graph, string bomRef)
    {
        if (graph is null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        return graph.Nodes.SingleOrDefault(x => x.Purl.Equals(bomRef, StringComparison.OrdinalIgnoreCase));
    }
}
