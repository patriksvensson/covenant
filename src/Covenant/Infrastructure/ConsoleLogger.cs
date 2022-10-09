namespace Covenant.Infrastructure;

internal sealed class ConsoleLogger : ILogger
{
    private readonly IAnsiConsole _console;

    public ConsoleLogger(IAnsiConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public void Write(LogLevel level, string text)
    {
        var now = DateTime.Now.ToString("HH:mm:ss");
        var desc = level switch
        {
            LogLevel.Information => "INF",
            LogLevel.Warning => "WAR",
            LogLevel.Error => "ERR",
            _ => "???",
        };

        _console.MarkupLine($"[[{desc}]] [[[grey]{now}[/]]] " + text);
    }
}