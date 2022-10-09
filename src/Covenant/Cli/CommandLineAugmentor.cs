namespace Covenant.Cli;

internal sealed class CommandLineAugmentor : ICommandLineAugmentor, ICommandLineResolver
{
    private readonly Command _command;
    private readonly Dictionary<string, Option> _options;

    public System.CommandLine.Parsing.ParseResult? ParseResult { get; set; }

    public CommandLineAugmentor(Command command)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
        _options = new Dictionary<string, Option>();
    }

    public void AddOption<T>(string alias, string description, object? defaultValue)
    {
        var option = new Option<T>(new[] { alias }, description);
        if (defaultValue != null)
        {
            option.SetDefaultValue(defaultValue);
        }

        _options.Add(alias, option);
        _command.AddOption(option);
    }

    public T? GetOption<T>(string alias)
    {
        if (ParseResult == null)
        {
            throw new InvalidOperationException("Parse result has not been initialized");
        }

        var option = _options[alias];
        var result = ParseResult.GetValueForOption(option);
        if (result == null)
        {
            return default;
        }

        return (T?)result;
    }
}
