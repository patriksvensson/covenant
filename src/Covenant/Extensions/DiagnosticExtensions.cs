namespace Covenant;

public static class DiagnosticExtensions
{
    public static string ToMarkup(this Diagnostic diagnostic)
    {
        if (diagnostic is null)
        {
            throw new ArgumentNullException(nameof(diagnostic));
        }

        var color = diagnostic.Kind switch
        {
            DiagnosticKind.Error => "red",
            DiagnosticKind.Warning => "yellow",
            DiagnosticKind.Info => "blue",
            _ => "default",
        };

        var properties = new List<string>();
        foreach (var (key, value) in diagnostic.Context)
        {
            properties.Add($"{key}[grey]=[/][blue]{value}[/]");
        }

        var ctx = string.Join("[grey],[/] ", properties);

        return $"[{color}]{diagnostic.Kind}[/]: {diagnostic.Message} [grey]=>[/] {ctx}";
    }

    public static IRenderable ToTable(this IReadOnlyCollection<Diagnostic> diagnostics)
    {
        if (diagnostics is null)
        {
            throw new ArgumentNullException(nameof(diagnostics));
        }

        var table = new Table();
        table.Caption(diagnostics.Count.ToString() + " entries");
        table.AddColumn(new TableColumn("Severity").Centered());
        table.AddColumn("Context");
        table.AddColumn("Message");

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Kind == DiagnosticKind.Info)
            {
                continue;
            }

            var color = diagnostic.Kind switch
            {
                DiagnosticKind.Error => Color.Red,
                DiagnosticKind.Warning => Color.Yellow,
                DiagnosticKind.Info => Color.Blue,
                _ => Color.Default,
            };

            table.AddRow(
                new Text(diagnostic.Kind.ToString(), new Style(foreground: color)),
                new Text(diagnostic.GetContext("Scope")?.ToString() ?? "N/A"),
                new Markup(diagnostic.Message));
        }

        return table;
    }

    public static IRenderable ToFullTable(this IReadOnlyCollection<Diagnostic> diagnostics, string? title = null)
    {
        if (diagnostics is null)
        {
            throw new ArgumentNullException(nameof(diagnostics));
        }

        var properties = new HashSet<string>(
            diagnostics.SelectMany(x => x.Context.Keys),
            StringComparer.OrdinalIgnoreCase);

        var table = new Table();

        if (title != null)
        {
            table.Title(title);
        }

        var errorCount = diagnostics.Count(x => x.Kind == DiagnosticKind.Error);
        var warningCount = diagnostics.Count(x => x.Kind == DiagnosticKind.Warning);
        table.Caption($"[red]{errorCount} errors[/], [yellow]{warningCount} warnings[/]");

        table.AddColumn("[yellow]Message[/]");

        foreach (var property in properties)
        {
            table.AddColumn("[yellow]" + property + "[/]");
        }

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Kind == DiagnosticKind.Info)
            {
                continue;
            }

            var color = diagnostic.Kind switch
            {
                DiagnosticKind.Error => Color.Red,
                DiagnosticKind.Warning => Color.Yellow,
                DiagnosticKind.Info => Color.Blue,
                _ => Color.Default,
            };

            var columns = new List<IRenderable>();
            columns.Add(new Markup(diagnostic.Message, new Style(color)));

            foreach (var property in properties)
            {
                if (diagnostic.Context.ContainsKey(property))
                {
                    columns.Add(new Text(diagnostic.Context[property]?.ToString() ?? string.Empty).Overflow(Overflow.Ellipsis));
                }
                else
                {
                    columns.Add(new EmptyRenderable());
                }
            }

            table.AddRow(columns);
        }

        return table;
    }
}

public sealed class EmptyRenderable : IRenderable
{
    public Measurement Measure(RenderContext context, int maxWidth)
    {
        return new Measurement(0, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderContext context, int maxWidth)
    {
        yield return Segment.Empty;
    }
}