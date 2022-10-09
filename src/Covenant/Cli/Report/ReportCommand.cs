namespace Covenant.Cli;

internal sealed class ReportCommand
{
    private readonly IAnsiConsole _console;
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly ReportGenerator _reportGenerator;

    public ReportCommand(
        IAnsiConsole console,
        IEnvironment environment,
        IFileSystem fileSystem,
        ReportGenerator reportGenerator)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
    }

    public int Run(ReportCommandSettings settings)
    {
        var input = settings.Input.MakeAbsolute(_environment);
        var bom = ReadBom(input);
        if (bom == null)
        {
            return -1;
        }

        // Generate HTML report
        var html = _reportGenerator.Generate(bom);

        // Write HTML to disk
        var output = GetOutputFilename(settings, input);
        _fileSystem.WriteAllText(output, html);
        _console.MarkupLine($"Wrote [blue]HTML[/] report to [yellow]{output}[/]");

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

    private FilePath GetOutputFilename(ReportCommandSettings settings, FilePath input)
    {
        // Got an output path?
        if (settings.Output != null)
        {
            return settings.Output.MakeAbsolute(_environment);
        }

        var filename = input.GetFilename()
            .RemoveAllExtensions()
            .ChangeExtension(".html");

        return input.GetDirectory()
            .CombineWithFilePath(filename)
            .MakeAbsolute(_environment);
    }
}
