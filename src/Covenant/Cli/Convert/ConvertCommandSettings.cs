namespace Covenant.Cli;

internal sealed class ConvertCommandSettings
{
    public ICommandLineResolver Resolver { get; }
    public BomSerializer Serializer { get; }

    public FilePath Input { get; }
    public FilePath? Output { get; set; }

    public ConvertCommandSettings(FilePath input, BomSerializer serializer, ICommandLineResolver resolver)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }
}
