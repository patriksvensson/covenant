namespace Covenant.Infrastructure;

public abstract class DiagnosticContext
{
    private readonly Stack<string> _scope;
    private readonly List<Diagnostic> _diagnostics;

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasErrors => Diagnostics.Any(d => d.Kind == DiagnosticKind.Error);

    protected DiagnosticContext()
    {
        _scope = new Stack<string>();
        _diagnostics = new List<Diagnostic>();
    }

    private sealed class DiagnosticScope : IDisposable
    {
        private readonly DiagnosticContext _context;

        public DiagnosticScope(DiagnosticContext context, string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _context = context;
            _context.PushScope(name);
        }

        ~DiagnosticScope()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.PopScope();
        }
    }

    public IDisposable Scope(string name)
    {
        return new DiagnosticScope(this, name);
    }

    private void PushScope(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _scope.Push(name);
    }

    private void PopScope()
    {
        if (_scope.Count > 0)
        {
            _scope.Pop();
        }
    }

    public Diagnostic AddError(string message)
    {
        var diagnostic = new Diagnostic(DiagnosticKind.Error, message ?? string.Empty);
        if (_scope.Count > 0)
        {
            diagnostic.WithContext("Scope", string.Join("/", _scope.Reverse()));
        }

        _diagnostics.Add(diagnostic);
        return diagnostic;
    }

    public Diagnostic AddWarning(string message)
    {
        var diagnostic = new Diagnostic(DiagnosticKind.Warning, message ?? string.Empty);
        if (_scope.Count > 0)
        {
            diagnostic.WithContext("Scope", string.Join("/", _scope.Reverse()));
        }

        _diagnostics.Add(diagnostic);
        return diagnostic;
    }

    public Diagnostic AddInfo(string message)
    {
        var diagnostic = new Diagnostic(DiagnosticKind.Info, message ?? string.Empty);
        if (_scope.Count > 0)
        {
            diagnostic.WithContext("Scope", string.Join("/", _scope.Reverse()));
        }

        _diagnostics.Add(diagnostic);
        return diagnostic;
    }
}