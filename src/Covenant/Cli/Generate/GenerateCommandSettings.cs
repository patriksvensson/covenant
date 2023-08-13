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

    public GenerateCommandSettings(ICommandLineResolver resolver)
    {
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }
}
