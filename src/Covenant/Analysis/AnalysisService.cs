namespace Covenant.Analysis;

public sealed class AnalysisService
{
    private readonly IAnsiConsole _console;
    private readonly IGlobber _globber;
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly List<Analyzer> _analyzers;

    public AnalysisService(
        IAnsiConsole console,
        IGlobber globber,
        IFileSystem fileSystem,
        IEnvironment environment,
        IEnumerable<Analyzer> analyzers)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _globber = globber ?? throw new ArgumentNullException(nameof(globber));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _analyzers = new List<Analyzer>(analyzers ?? Enumerable.Empty<Analyzer>());
    }

    public Bom? Analyze(AnalysisSettings settings, out IReadOnlyList<Diagnostic> diagnostics)
    {
        var graph = new Graph<BomComponent>(BomComponentComparer.Shared);
        diagnostics = new List<Diagnostic>();

        var root = GetRoot(settings);
        var components = new HashSet<BomComponent>();
        var dependencies = new HashSet<BomDependency>();

        foreach (var analyzer in _analyzers)
        {
            analyzer.BeforeAnalysis(settings);

            if (!analyzer.Enabled)
            {
                continue;
            }

            var context = new AnalysisContext(root, graph, settings);

            foreach (var path in GetFilePaths(analyzer, settings))
            {
                context.Reset();

                if (analyzer.CanHandle(context, path))
                {
                    var relative = root.GetRelativePath(path);
                    _console.MarkupLine($"  [grey]>[/] Analyzing [yellow]{relative}[/]...");

                    // Analyze
                    analyzer.Analyze(context, path);

                    // Add all new components
                    components.AddRange(context.Delta.Nodes);

                    // Add all new dependencies
                    foreach (var node in context.Delta.Nodes)
                    {
                        var nodeDependencies = context.Delta
                            .GetOutgoingNodes(node)
                            .Select(n => n.Purl)
                            .ToList();

                        if (nodeDependencies.Count > 0)
                        {
                            dependencies.Add(new BomDependency(node.Purl)
                            {
                                Dependencies = nodeDependencies,
                            });
                        }
                    }

                    // Merge diagnostics
                    diagnostics = diagnostics.Merge(context.Diagnostics);
                }
            }

            analyzer.AfterAnalysis(settings);
        }

        return new Bom(settings.Name ?? "Unknown", settings.Version ?? "0.0.0")
        {
            Components = new List<BomComponent>(components),
            Dependencies = new List<BomDependency>(dependencies),
            Metadata = settings.Metadata?.Select(
                pair => new BomMetadata(pair.Key, pair.Value))?.ToList() ?? new List<BomMetadata>(),
        };
    }

    private DirectoryPath GetRoot(AnalysisSettings settings)
    {
        var root = _environment.WorkingDirectory;

        if (settings.Input != null)
        {
            // Is this a directory?
            if (_fileSystem.Directory.Exists(settings.Input))
            {
                // Use it as glob root
                root = new DirectoryPath(settings.Input);
            }
        }

        return root;
    }

    private IEnumerable<FilePath> GetFilePaths(Analyzer analyzer, AnalysisSettings settings)
    {
        var root = _environment.WorkingDirectory;

        if (settings.Input != null)
        {
            // Is this a file?
            if (_fileSystem.File.Exists(settings.Input))
            {
                yield return new FilePath(settings.Input).MakeAbsolute(_environment);
                yield break;
            }

            // Is this a directory?
            if (_fileSystem.Directory.Exists(settings.Input))
            {
                // Use it as glob root
                root = new DirectoryPath(settings.Input);
            }
        }

        foreach (var pattern in analyzer.Patterns)
        {
            foreach (var path in _globber.Match(pattern, new GlobberSettings()
            {
                Root = root,
                Predicate = (directory) => _analyzers.All(a => a.ShouldTraverse(directory.Path)),
            }))
            {
                if (path is FilePath file)
                {
                    yield return file;
                }
            }
        }
    }
}
