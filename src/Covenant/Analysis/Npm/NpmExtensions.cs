namespace Covenant.Analysis.Npm;

internal static class NpmExtensions
{
    public static BomComponent? FindNpmComponent(this IReadOnlyGraph<BomComponent> graph, string name, NpmVersionRange range, out bool foundMatch)
    {
        var nodes = graph.Nodes.OfType<NpmComponent>()
            .Where(c => c.Name.Equals(name, StringComparison.Ordinal))
            .ToArray();

        var version = range.Matches(nodes.Select(c => c.Data));
        if (version == null)
        {
            foundMatch = false;

            if (nodes.Length == 1)
            {
                return nodes[0];
            }
        }

        foundMatch = true;
        return graph.Nodes.OfType<NpmComponent>()
            .Where(c => c.Name.Equals(name, StringComparison.Ordinal))
            .FirstOrDefault(x => x.Data is NpmSemVersion v && v.Equals(version));
    }
}
