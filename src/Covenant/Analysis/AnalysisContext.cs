namespace Covenant.Analysis;

public sealed class AnalysisContext : DiagnosticContext
{
    private readonly Graph<BomComponent> _graph;
    private readonly Graph<BomComponent> _localGraph;
    private readonly HashSet<BomFile> _files;
    private readonly HashSet<BomFile> _localFiles;

    public IReadOnlyGraph<BomComponent> Graph => _graph;
    public IReadOnlyGraph<BomComponent> Delta => _localGraph;
    public IReadOnlySet<BomFile> Files => _files;
    public IReadOnlySet<BomFile> DeltaFiles => _localFiles;
    public DirectoryPath Root { get; }
    public ICommandLineResolver Cli { get; }
    public CovenantConfiguration Configuration { get; }

    public AnalysisContext(
        DirectoryPath root,
        Graph<BomComponent> graph,
        AnalysisSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _localGraph = new Graph<BomComponent>(BomComponentComparer.Shared);

        Root = root ?? throw new ArgumentNullException(nameof(root));
        Cli = settings.Cli;
        Configuration = settings.Configuration;

        _files = new HashSet<BomFile>();
        _localFiles = new HashSet<BomFile>();
    }

    internal void Reset()
    {
        _localGraph.Clear();
        _localFiles.Clear();
    }

    public BomComponent AddComponent(BomComponent component)
    {
        // Already known?
        if (_graph.Exist(component))
        {
            return component;
        }

        // Add it to the local graph.
        _localGraph.Add(component);
        return _graph.Add(component);
    }

    public BomFile AddFile(BomFile file)
    {
        if (_files.Contains(file))
        {
            _localFiles.Add(file);
            _files.Add(file);
        }

        return file;
    }

    public void Connect(BomComponent start, BomComponent end, string? metadata = null)
    {
        _localGraph.Connect(start, end, metadata);
    }
}