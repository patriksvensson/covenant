namespace Covenant.Analysis.Npm;

internal abstract class NpmVersion
{
    public string OriginalVersion { get; }

    protected NpmVersion(string originalString)
    {
        OriginalVersion = originalString;
    }

    public static NpmVersion Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Version cannot be empty");
        }

        if (char.IsDigit(text[0]))
        {
            if (SemVersion.TryParse(text, SemVersionStyles.Any, out var result))
            {
                return new NpmSemVersion(result);
            }
        }

        return new NpmTextVersion(text);
    }
}

internal sealed class NpmSemVersion : NpmVersion
{
    public SemVersion Version { get; }

    public NpmSemVersion(SemVersion version)
        : base(version.ToString())
    {
        Version = version;
    }
}

internal sealed class NpmTextVersion : NpmVersion
{
    public string Content { get; }

    public NpmTextVersion(string content)
        : base(content)
    {
        Content = content;
    }
}
