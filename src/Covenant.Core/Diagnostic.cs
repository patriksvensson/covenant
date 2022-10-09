namespace Covenant.Infrastructure;

public sealed class Diagnostic
{
    public DiagnosticKind Kind { get; }
    public string Message { get; }
    public Dictionary<string, object> Context { get; }

    public Diagnostic(DiagnosticKind kind, string message)
    {
        Kind = kind;
        Message = message;
        Context = new Dictionary<string, object>();
    }

    public object? GetContext(string name)
    {
        Context.TryGetValue(name, out var value);
        return value;
    }

    public Diagnostic WithContext(string key, object value)
    {
        Context.Add(key, value);
        return this;
    }
}