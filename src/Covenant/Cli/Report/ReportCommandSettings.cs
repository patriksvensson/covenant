namespace Covenant.Cli;

internal sealed class ReportCommandSettings
{
    public FilePath Input { get; }
    public FilePath? Output { get; set; }

    public ReportCommandSettings(FilePath input)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }
}