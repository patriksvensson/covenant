using System.CommandLine.IO;

namespace Covenant.Infrastructure;

internal sealed class AnsiConsoleStreamWriter : IStandardStreamWriter
{
    private readonly IAnsiConsole _console;

    public AnsiConsoleStreamWriter(IAnsiConsole console)
    {
        _console = console;
    }

    public void Write(string? value)
    {
        _console.Write(value!);
    }
}
