namespace Covenant.Analysis.Dotnet;

internal static class DotnetExtensions
{
    public static BomComponent? FindDotnetComponent(this IReadOnlyGraph<BomComponent> graph, string name, VersionRange range)
    {
        var nodes = graph.Nodes.OfType<DotnetComponent>()
            .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var version = range.FindBestMatch(nodes.Select(c => c.Data));
        if (version == null)
        {
            if (nodes.Length == 1)
            {
                return nodes[0];
            }

            return null;
        }

        return graph.Nodes.OfType<DotnetComponent>()
            .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(x => x.Data.Equals(version));
    }
}
