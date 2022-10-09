namespace Covenant.Cli;

internal sealed class CheckCommand
{
    private readonly IAnsiConsole _console;
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly CovenantConfigurationReader _reader;
    private readonly ComplianceChecker _checker;

    public CheckCommand(
        IAnsiConsole console,
        IEnvironment environment,
        IFileSystem fileSystem,
        CovenantConfigurationReader reader,
        ComplianceChecker checker)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _checker = checker ?? throw new ArgumentNullException(nameof(checker));
    }

    public int Run(CheckCommandSettings settings)
    {
        var input = settings.Input.MakeAbsolute(_environment);
        var bom = ReadBom(input);
        if (bom == null)
        {
            return -1;
        }

        var configurationPath = settings.Configuration ?? input.GetDirectory().CombineWithFilePath("covenant.config");
        var configuration = _reader.Read(configurationPath.MakeAbsolute(_environment));
        if (configuration == null)
        {
            // Create an empty configuration
            configuration = new CovenantConfiguration();
        }

        var context = new ComplianceCheckerContext(configuration);
        _checker.PerformComplianceCheck(bom, context);

        // Any errors?
        if (context.Diagnostics.Any(c => c.Kind == DiagnosticKind.Error))
        {
            _console.Write(context.Diagnostics.ToFullTable());
            _console.WriteLine();
            return 1;
        }

        // Any warnings?
        if (context.Diagnostics.Any(c => c.Kind == DiagnosticKind.Warning))
        {
            _console.Write(context.Diagnostics.ToFullTable());
            _console.WriteLine();
        }

        return 0;
    }

    private Bom? ReadBom(FilePath input)
    {
        if (!_fileSystem.Exist(input))
        {
            _console.MarkupLine($"[red]The file [u]{input}[/] do not exist[/]");
            return null;
        }

        var json = _fileSystem.ReadAllText(input);
        if (json == null)
        {
            _console.MarkupLineInterpolated($"[red]Could not read [u]{input}[/][/]");
            return null;
        }

        var bom = BomReader.Read(json);
        if (bom == null)
        {
            _console.MarkupLine($"[red]Could not read Covenant BOM from [u]{input}[/][/]");
            return null;
        }

        return bom;
    }
}
