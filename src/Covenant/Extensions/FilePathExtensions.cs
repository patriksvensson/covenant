namespace Covenant;

internal static class FilePathExtensions
{
    public static FilePath RemoveAllExtensions(this FilePath path)
    {
        while (path.HasExtension)
        {
            path = path.RemoveExtension();
        }

        return path;
    }
}
