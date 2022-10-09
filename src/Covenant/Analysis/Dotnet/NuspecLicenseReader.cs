using NuGet.Packaging;

namespace Covenant.Analysis.Dotnet;

public sealed class NuspecLicenseReader
{
    private readonly IFileSystem _fileSystem;

    public NuspecLicenseReader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public bool TryReadLicense(FilePath path, [NotNullWhen(true)] out NuspecLicense? license)
    {
        var file = _fileSystem.GetFile(path);
        if (!file.Exists)
        {
            license = null;
            return false;
        }

        var licenseMetadata = ReadLicenseMetadata(file, out var nuspec);

        var copyright = nuspec?.GetCopyright();
        if (string.IsNullOrWhiteSpace(copyright))
        {
            copyright = null;
        }

        if (licenseMetadata == null)
        {
            license = new NuspecLicense
            {
                Url = nuspec?.GetLicenseUrl(),
                Copyright = copyright,
            };

            return true;
        }

        if (licenseMetadata.Type == LicenseType.File)
        {
            path = path.GetDirectory().CombineWithFilePath(licenseMetadata.License);
            file = _fileSystem.File.Retrieve(path);
            if (file.Exists)
            {
                using (var licenseStream = file.OpenRead())
                using (var reader = new StreamReader(licenseStream))
                {
                    license = new NuspecLicense
                    {
                        Url = nuspec?.GetLicenseUrl(),
                        Text = reader.ReadToEnd(),
                        Copyright = copyright,
                    };

                    return true;
                }
            }
        }

        license = new NuspecLicense
        {
            Id = licenseMetadata.License,
            Url = nuspec?.GetLicenseUrl(),
            Copyright = copyright,
        };

        return true;
    }

    private static LicenseMetadata? ReadLicenseMetadata(IFile file, out NuspecReader? nuspec)
    {
        using (var stream = file.OpenRead())
        {
            var reader = new PackageArchiveReader(stream);
            nuspec = reader.NuspecReader;
            return nuspec.GetLicenseMetadata();
        }
    }
}
