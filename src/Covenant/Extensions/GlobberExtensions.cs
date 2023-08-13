namespace Covenant;

internal static class GlobberExtensions
{
    public static FilePath[] GetFiles(
        this IGlobber globber,
        DirectoryPath root,
        string pattern)
    {
        if (globber == null)
        {
            throw new ArgumentNullException(nameof(globber));
        }

        return globber.Match(pattern, new GlobberSettings
        {
            Root = root,
        })
        .OfType<FilePath>()
        .ToArray();
    }
}
