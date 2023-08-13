namespace Covenant;

internal static class FileSystemExtensions
{
    public static string ReadAllText(this IFileSystem fileSystem, FilePath path)
    {
        if (fileSystem is null)
        {
            throw new ArgumentNullException(nameof(fileSystem));
        }

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        using (var stream = fileSystem.File.OpenRead(path))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    public static void WriteAllText(this IFileSystem fileSystem, FilePath path, string text)
    {
        if (fileSystem is null)
        {
            throw new ArgumentNullException(nameof(fileSystem));
        }

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        using (var stream = fileSystem.File.OpenWrite(path))
        using (var reader = new StreamWriter(stream))
        {
            reader.Write(text);
        }
    }
}
