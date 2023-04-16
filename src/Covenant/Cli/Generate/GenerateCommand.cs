namespace Covenant.Cli;

internal sealed class GenerateCommand
{
    private readonly IAnsiConsole _console;
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly AnalysisService _analysis;
    private readonly CovenantConfigurationReader _reader;
    private readonly IReadOnlyList<ICovenantMiddleware> _middlewares;

    public GenerateCommand(
        IAnsiConsole console,
        IEnvironment environment,
        IFileSystem fileSystem,
        AnalysisService analysis,
        CovenantConfigurationReader reader,
        IEnumerable<ICovenantMiddleware> middleware)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _middlewares = new List<ICovenantMiddleware>(middleware);
    }

    public int Analyze(GenerateCommandSettings settings)
    {
        _console.Write(new FigletText("Covenant").Color(Color.Yellow));

        // Perform analysis
        var (result, diagnostics) = _console.Status()
            .Start("Analyzing...", ctx =>
            {
                var configuration = GetConfiguration(settings);

                var result = _analysis.Analyze(
                    new AnalysisSettings(configuration, settings.Resolver)
                    {
                        Input = settings.Input,
                        Name = GetName(settings),
                        Version = settings.Version,
                        Output = settings.Output,
                        Reporter = new ProgressReporter(ctx),
                        Metadata = settings.Metadata,
                    },
                    out var diagnostics);

                // Make sure we have a BOM
                result ??= new Bom(GetName(settings), settings.Version ?? "0.0.0");

                // Process the BOM using middleware
                // TODO: refine this a bit later
                ctx.Status("Processing...");
                var middlewareCtx = new MiddlewareContext(GetInputPath(settings).FullPath, configuration, settings.Resolver);
                foreach (var middleware in _middlewares.OrderBy(m => m.Order))
                {
                    result = middleware.Process(middlewareCtx, result);
                }

                // Merge diagnostics
                diagnostics = diagnostics.Merge(middlewareCtx.Diagnostics);

                return (result, diagnostics);
            });

        // Any errors?
        if (diagnostics.Any(c => c.Kind == DiagnosticKind.Error))
        {
            _console.Write(diagnostics.ToTable());
            _console.WriteLine();
            return 1;
        }

        // Any warnings?
        if (diagnostics.Any(c => c.Kind == DiagnosticKind.Warning))
        {
            _console.Write(diagnostics.ToTable());
            _console.WriteLine();
        }

        // No result?
        if (result == null)
        {
            _console.Write(diagnostics.ToTable());
            _console.WriteLine();
            _console.MarkupLine("[red]Could not generate SBOM[/]");
            return 2;
        }

        // No entries in result?
        if (result.Components.Count == 0)
        {
            _console.WriteLine();
            _console.MarkupLine("[red]SBOM was not created since no components could be found[/]");
            return 3;
        }

        // Get the output filename
        var json = BomWriter.Write(result);
        if (json == null)
        {
            _console.WriteLine();
            _console.MarkupLine("[red]An error occured while serializing SBOM[/]");
            return 4;
        }

        var output = GetOutputPath(settings);
        _fileSystem.WriteAllText(output, json);

        _console.WriteLine();
        _console.MarkupLine($"Wrote [blue]Covenant[/] SBOM to [yellow]{output.FullPath}[/]");
        return 0;
    }

    private CovenantConfiguration GetConfiguration(GenerateCommandSettings settings)
    {
        var configurationPath = settings.Configuration ?? GetInputPath(settings).CombineWithFilePath("covenant.config");
        var configuration = _reader.Read(configurationPath.MakeAbsolute(_environment));
        if (configuration == null)
        {
            // Create an empty configuration
            configuration = new CovenantConfiguration();
        }

        return configuration;
    }

    private DirectoryPath GetInputPath(GenerateCommandSettings settings)
    {
        if (settings.Input != null)
        {
            return new DirectoryPath(settings.Input)
                .MakeAbsolute(_environment);
        }

        return _environment.WorkingDirectory.MakeAbsolute(_environment);
    }

    private FilePath GetOutputPath(GenerateCommandSettings settings)
    {
        // Got an output path?
        if (settings.Output != null)
        {
            return new FilePath(settings.Output)
                .MakeAbsolute(_environment);
        }

        var root = _environment.WorkingDirectory;

        // Got an input path?
        if (!string.IsNullOrWhiteSpace(settings.Input)
            && _fileSystem.Directory.Exists(settings.Input))
        {
            root = new DirectoryPath(settings.Input).MakeAbsolute(_environment);
        }

        var filename = new FilePath(GetName(settings))
            .ChangeExtension("covenant.json");

        return root
            .CombineWithFilePath(filename)
            .MakeAbsolute(_environment);
    }

    private string GetName(GenerateCommandSettings settings)
    {
        if (settings.Name != null)
        {
            return settings.Name;
        }

        if (settings.Input != null)
        {
            if (_fileSystem.Directory.Exists(settings.Input))
            {
                return new DirectoryPath(settings.Input).GetDirectoryName();
            }

            var path = new FilePath(settings.Input);
            return path.GetFilenameWithoutExtension().FullPath;
        }

        return "Unknown";
    }
}
