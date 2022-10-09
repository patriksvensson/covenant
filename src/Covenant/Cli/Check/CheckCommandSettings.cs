namespace Covenant.Cli;

internal sealed class CheckCommandSettings
{
    public FilePath Input { get; }
    public FilePath? Configuration { get; set; }

    public CheckCommandSettings(FilePath input)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }
}