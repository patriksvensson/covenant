using Covenant.Cli;
using Path = Spectre.IO.Path;

namespace Covenant;

public sealed class AnalysisConfiguration
{
    public CovenantConfiguration Configuration { get; }

    public DirectoryPath Root { get; }
    public HashSet<DirectoryPath> ExcludedDirectories { get; }
    public HashSet<FilePath> ExcludedFiles { get; }

    public AnalysisConfiguration(
        IFileSystem fileSystem,
        IEnvironment environment,
        GenerateCommandSettings settings,
        CovenantConfiguration configuration)
    {
        Configuration = configuration;
        ExcludedDirectories = new HashSet<DirectoryPath>(PathComparer.Default);
        ExcludedFiles = new HashSet<FilePath>(PathComparer.Default);

        Root = GetInputPath(fileSystem, environment, settings);
        foreach (var path in configuration.Exclude.Paths)
        {
            var directoryPath = new DirectoryPath(path).MakeAbsolute(Root);
            if (fileSystem.Directory.Exists(directoryPath))
            {
                ExcludedDirectories.Add(directoryPath);
            }
            else
            {
                ExcludedFiles.Add(
                    new FilePath(path)
                        .MakeAbsolute(Root));
            }
        }
    }

    private static DirectoryPath GetInputPath(
        IFileSystem fileSystem,
        IEnvironment environment,
        GenerateCommandSettings settings)
    {
        if (settings.Input != null)
        {
            // Is this a directory?
            var input = new DirectoryPath(settings.Input).MakeAbsolute(environment);
            if (fileSystem.Directory.Exists(input))
            {
                return input;
            }
        }

        return environment.WorkingDirectory;
    }
}