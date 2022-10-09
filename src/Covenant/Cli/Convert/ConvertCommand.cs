namespace Covenant.Cli;

internal sealed class ConvertCommand
{
    private readonly IAnsiConsole _console;
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public ConvertCommand(
        IAnsiConsole console,
        IEnvironment environment,
        IFileSystem fileSystem)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public int Convert(ConvertCommandSettings settings)
    {
        return _console.Status()
            .Start("Converting...", _ =>
            {
                var input = settings.Input.MakeAbsolute(_environment);
                var bom = ReadBom(input);
                if (bom == null)
                {
                    return -1;
                }

                // Serialize SBOM file
                var data = settings.Serializer.Serialize(bom, new BomSerializerSettings
                {
                    Name = bom.Name,
                    Version = bom.Version,
                }, settings.Resolver);

                // Write the SBOM file
                var output = GetOutputFilename(settings, input).MakeAbsolute(_environment);
                File.WriteAllText(output.FullPath, data);
                _console.MarkupLineInterpolated($"Wrote [blue]{settings.Serializer.Name}[/] SBOM to [yellow]{output}[/]");

                return 0;
            });
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

    private FilePath GetOutputFilename(ConvertCommandSettings settings, FilePath input)
    {
        // Got an output path?
        if (settings.Output != null)
        {
            return settings.Output.MakeAbsolute(_environment);
        }

        var filename = input.GetFilename()
            .RemoveAllExtensions()
            .ChangeExtension(settings.Serializer.Extension);

        return input
            .GetDirectory()
            .CombineWithFilePath(filename)
            .MakeAbsolute(_environment);
    }
}
