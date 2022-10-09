namespace Covenant.Reporting;

public sealed class ReportComponent
{
    private readonly BomComponent _component;

    public string Ref => _component.Purl;
    public string Name => _component.Name;
    public string Version => _component.Version;
    public BomLicense? License => _component.License;
    public BomComponentKind Kind => _component.Kind;

    public List<BomComponent> Dependencies { get; }
    public bool HasDependencies => Dependencies.Count > 0;

    public ReportComponent(BomComponent component, List<BomComponent> dependencies)
    {
        _component = component ?? throw new ArgumentNullException(nameof(component));
        Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }
}
