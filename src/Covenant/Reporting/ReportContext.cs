namespace Covenant.Reporting;

public sealed class ReportContext
{
    private readonly Bom _bom;
    private readonly IReadOnlyList<ReportComponent> _components;
    private readonly IReadOnlyList<ReportComponent> _dependencies;
    private readonly IReadOnlyList<ReportFile> _files;

    public string Name => _bom.Name;
    public string Version => _bom.Version;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needed for templating")]
    public string CovenantVersion => Infrastructure.CovenantVersion.Version;

    public IEnumerable<ReportComponent> Components => _components.OrderBy(c => c.Name);
    public IEnumerable<ReportComponent> Dependencies => _dependencies.OrderBy(c => c.Name);
    public IEnumerable<ReportFile> Files => _files.OrderBy(c => c.Path);
    public IEnumerable<BomMetadata> Metadata => _bom.Metadata;
    public ReportLicenseInformation Licenses { get; }

    public int ComponentCount => _components.Count;
    public int DependencyCount => _dependencies.Count;
    public int FileCount => _files.Count;
    public int MetadataCount => _bom.Metadata.Count;

    public ReportContext(Bom bom)
    {
        _bom = bom ?? throw new ArgumentNullException(nameof(bom));
        _components = GetComponents(_bom, BomComponentKind.Root);
        _dependencies = GetComponents(_bom, BomComponentKind.Library);
        _files = _bom.Files.Select(f => new ReportFile(f)).ToList();

        Licenses = ReportLicenseInformation.Create(_bom);
    }

    private static IReadOnlyList<ReportComponent> GetComponents(Bom bom, BomComponentKind kind)
    {
        var result = new List<ReportComponent>();

        foreach (var component in bom.Components)
        {
            if (component.Kind != kind)
            {
                continue;
            }

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
