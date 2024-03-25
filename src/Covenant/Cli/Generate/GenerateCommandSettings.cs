namespace Covenant.Cli;

public sealed class GenerateCommandSettings
{
    public ICommandLineResolver Resolver { get; }

    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public FilePath? Configuration { get; set; }
    public bool NoLogo { get; set; }

    public GenerateCommandSettings(ICommandLineResolver resolver)
    {
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public DirectoryPath GetResolvedInputPath(
        IFileSystem fileSystem,
        IEnvironment environment)
    {
        if (Input != null)
        {
            // Is this a directory?
            var directoryPath = new DirectoryPath(Input).MakeAbsolute(environment);
            if (fileSystem.Directory.Exists(directoryPath))
            {
                return directoryPath;
            }
        }

        return environment.WorkingDirectory;
    }
}
