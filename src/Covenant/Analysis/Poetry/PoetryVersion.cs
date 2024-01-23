namespace Covenant.Analysis.Poetry;

internal abstract class PoetryVersion
{
    public string OriginalVersion { get; }

    protected PoetryVersion(string originalString)
    {
        OriginalVersion = originalString;
    }

    public static PoetryVersion Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Version cannot be empty");
        }

        // TODO: Maybe unnecessary?
        if (char.IsDigit(text[0]))
        {
            if (SemVersion.TryParse(text, SemVersionStyles.Any, out var result))
            {
                return new PoetrySemVersion(result);
            }
        }

        return new PoetryTextVersion(text);
    }
}

internal sealed class PoetrySemVersion : PoetryVersion
{
    public SemVersion Version { get; }

    public PoetrySemVersion(SemVersion version)
        : base(version.ToString())
    {
        Version = version;
    }
}

internal sealed class PoetryTextVersion : PoetryVersion
{
    public string Content { get; }

    public PoetryTextVersion(string content)
        : base(content)
    {
        Content = content;
    }
}
