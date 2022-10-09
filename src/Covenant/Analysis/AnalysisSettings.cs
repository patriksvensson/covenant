namespace Covenant.Analysis;

public sealed class AnalysisSettings
{
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    public IProgress<string>? Reporter { get; set; }
    public ICommandLineResolver Cli { get; }

    public AnalysisSettings(ICommandLineResolver cli)
    {
        Cli = cli ?? throw new ArgumentNullException(nameof(cli));
    }
}
