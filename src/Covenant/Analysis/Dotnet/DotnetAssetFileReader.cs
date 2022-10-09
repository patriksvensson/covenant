namespace Covenant.Analysis.Dotnet;

internal sealed class DotnetAssetFileReader
{
    private readonly IFileSystem _fileSystem;

    public DotnetAssetFileReader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public AssetFile? ReadAssetFile(FilePath path)
    {
        var assetFile = _fileSystem.GetFile(path);
        if (!assetFile.Exists)
        {
            return null;
        }

        using (var stream = assetFile.OpenRead())
        {
            return AssetFile.FromStream(stream);
        }
    }
}
