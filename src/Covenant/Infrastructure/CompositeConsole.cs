using System.CommandLine.IO;

namespace Covenant.Infrastructure;

internal sealed class CompositeConsole : IConsole, IAnsiConsole
{
    private readonly AnsiConsoleStreamWriter _standardOut;
    private readonly IStandardStreamWriter _standardError;

    public CompositeConsole()
    {
        _standardOut = new AnsiConsoleStreamWriter(AnsiConsole.Console);
        _standardError = StandardStreamWriter.Create(Console.Error);
    }

    bool IStandardOut.IsOutputRedirected => Console.IsOutputRedirected;

    bool IStandardError.IsErrorRedirected => Console.IsErrorRedirected;

    bool IStandardIn.IsInputRedirected => Console.IsInputRedirected;

    IStandardStreamWriter IStandardOut.Out => _standardOut;

    IStandardStreamWriter IStandardError.Error => _standardError;

    public Profile Profile => AnsiConsole.Console.Profile;

    public IAnsiConsoleCursor Cursor => AnsiConsole.Console.Cursor;

    public IAnsiConsoleInput Input => AnsiConsole.Console.Input;

    public IExclusivityMode ExclusivityMode => AnsiConsole.Console.ExclusivityMode;

    public RenderPipeline Pipeline => AnsiConsole.Console.Pipeline;

    public void Clear(bool home)
    {
        AnsiConsole.Console.Clear(home);
    }

    public void Write(IRenderable renderable)
    {
        AnsiConsole.Console.Write(renderable);
    }
}