namespace Covenant.Analysis;

public sealed class AnalysisSettings
{
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    public IProgress<string>? Reporter { get; set; }
    public AnalysisConfiguration Configuration { get; }
    public ICommandLineResolver Cli { get; }

    public AnalysisSettings(
        AnalysisConfiguration configuration,
        ICommandLineResolver cli)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Cli = cli ?? throw new ArgumentNullException(nameof(cli));
    }
}
