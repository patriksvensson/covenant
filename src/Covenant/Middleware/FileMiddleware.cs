using System.Security.Cryptography;

namespace Covenant.Middleware;

internal sealed class FileMiddleware : ICovenantMiddleware
{
    private readonly IFileSystem _fileSystem;
    private readonly IGlobber _globber;

    public int Order => 500;

    public FileMiddleware(
        IFileSystem fileSystem,
        IGlobber globber)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _globber = globber ?? throw new ArgumentNullException(nameof(globber));
    }

    public Bom Process(MiddlewareContext context, Bom bom)
    {
        using var fileScope = context.Scope("File");

        foreach (var fileConfiguration in context.Configuration.Files)
        {
            if (fileConfiguration.Path == null)
            {
                continue;
            }

            var input = new DirectoryPath(context.InputPath);
            var files = _globber.GetFiles(input, fileConfiguration.Path);

            // No matched files?
            if (files.Length == 0)
            {
                context.AddWarning($"The pattern '{fileConfiguration.Path}' did not result in any files");
                continue;
            }

            // Create the representations
            foreach (var file in files)
            {
                var bomFile = new BomFile(
                    input.GetRelativePath(file).FullPath,
                    CalculateHash(file));

                if (fileConfiguration.License != null)
                {
                    SpdxLicense.TryGetById(fileConfiguration.License, out var license);

                    bomFile.License = new BomLicense
                    {
                        Id = license != null ? license.Id : null,
                        Name = license == null ? fileConfiguration.License : null,
                        Url = license != null ? $"https://spdx.org/licenses/{license.Id}.html" : null,
                    };
                }

                // Add the file
                bom.Files.Add(bomFile);
            }
        }

        return bom;
    }

    private BomHash CalculateHash(FilePath path)
    {
        using (var stream = _fileSystem.GetFile(path).OpenRead())
        {
            var hash = BitConverter.ToString(SHA1.Create().ComputeHash(stream));
            return new BomHash(BomHashAlgorithm.SHA1, hash.Replace("-", string.Empty));
        }
    }
}
