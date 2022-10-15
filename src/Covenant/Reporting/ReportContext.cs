namespace Covenant.Reporting;

public sealed class ReportContext
{
    private readonly Bom _bom;
    private readonly IReadOnlyList<ReportComponent> _components;
    private readonly IReadOnlyList<ReportComponent> _dependencies;

    public string Name => _bom.Name;
    public string Version => _bom.Version;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needed for templating")]
    public string CovenantVersion => Infrastructure.CovenantVersion.Version;

    public IEnumerable<ReportComponent> Components => _components.OrderBy(c => c.Name);
    public IEnumerable<ReportComponent> Dependencies => _dependencies.OrderBy(c => c.Name);
    public IEnumerable<BomMetadata> Metadata => _bom.Metadata;

    public int ComponentCount => _components.Count;
    public int DependencyCount => _dependencies.Count;
    public int MetadataCount => _bom.Metadata.Count;

    public Dictionary<string, int> Licenses { get; }
    public Dictionary<string, int> UnknownLicenses { get; }

    public int DistinctTextLicenseCount { get; }
    public int LicenseCount => Licenses.Count;
    public int NoLicenseCount { get; }
    public int UnknownLicenseCount => UnknownLicenses.Count + DistinctTextLicenseCount;

    public ReportContext(Bom bom)
    {
        _bom = bom ?? throw new ArgumentNullException(nameof(bom));

        var components = GetComponents(bom);

        _components = components.Where(c => c.Kind == BomComponentKind.Root).ToList();
        _dependencies = components.Where(c => c.Kind == BomComponentKind.Library).ToList();

        Licenses = _bom.Components
            .Where(c => c.License?.Id != null || c.License?.Expression != null)
            .OrderByDescending(c => c.License?.Id != null)
            .GroupBy(
                c => c.License?.Id
                    ?? c.License?.Expression
                    ?? throw new InvalidOperationException("Invalid license"),
                StringComparer.Ordinal)
            .ToDictionary(
                c => c.Key,
                v => v.Count(),
                StringComparer.Ordinal);

        DistinctTextLicenseCount = new HashSet<string>(
            _bom.Components
                .Where(c => c.License?.Text != null && c.License?.Id == null)
                .Select(c => c.License?.Text?.Hash!),
            StringComparer.Ordinal).Count;

        UnknownLicenses = _bom.Components
            .Where(c => c.License != null && c.License?.Id == null && c.License?.Expression == null && c.License?.Text == null)
            .GroupBy(
                c => c.License?.Id
                    ?? c.License?.Name
                    ?? c.License?.Url
                    ?? throw new InvalidOperationException("Invalid license"),
                StringComparer.Ordinal)
            .ToDictionary(
                c => c.Key,
                v => v.Count(),
                StringComparer.Ordinal);

        NoLicenseCount = _bom.Components
            .Where(c => c.Kind != BomComponentKind.Root)
            .Count(x => x.License == null);
    }

    private static IReadOnlyList<ReportComponent> GetComponents(Bom bom)
    {
        var result = new List<ReportComponent>();

        foreach (var component in bom.Components)
        {
            var dependencyResult = new List<BomComponent>();

            var root = bom.Dependencies.Find(x => x.Purl == component.Purl);
            if (root != null)
            {
                foreach (var dependency in root.Dependencies)
                {
                    var dependencyComponent = bom.Components.Find(x => x.Purl == dependency);
                    if (dependencyComponent != null)
                    {
                        dependencyResult.Add(dependencyComponent);
                    }
                }
            }

            result.Add(new ReportComponent(component, dependencyResult));
        }

        return result;
    }
}
