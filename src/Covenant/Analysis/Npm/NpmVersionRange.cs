namespace Covenant.Analysis.Npm;

internal sealed class NpmVersionRange
{
    private readonly SemVersionRange? _versionRangeSet;
    private readonly string? _text;

    public NpmVersionRange(string text)
    {
        if (!SemVersionRange.TryParseNpm(text, out _versionRangeSet))
        {
            _text = text;
        }
    }

    public NpmVersion? Matches(IEnumerable<NpmVersion> versions)
    {
        if (_versionRangeSet != null)
        {
            // Get all nodes
            var candidates = versions
                .OfType<NpmSemVersion>()
                .Where(x => _versionRangeSet.Contains(x.Version))
                .Select(x => x.Version)
                .ToList();

            if (candidates.Count > 0)
            {
                // Sort the list
                candidates.Sort(SemVersion.CompareSortOrder);
                var version = candidates.FirstOrDefault();
                if (version != null)
                {
                    return versions
                        .OfType<NpmSemVersion>()
                        .FirstOrDefault(x => x.Version.Equals(version));
                }
            }
        }
        else
        {
            foreach (var item in versions.OfType<NpmTextVersion>())
            {
                if (item.Content.Equals(_text, StringComparison.Ordinal))
                {
                    return item;
                }
            }
        }

        return null;
    }
}
