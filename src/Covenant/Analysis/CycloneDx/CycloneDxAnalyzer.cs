namespace Covenant.Analysis.CycloneDx;

using CycloneBom = CycloneDX.Models.Bom;
using CycloneComponent = CycloneDX.Models.Component;
using CycloneHash = CycloneDX.Models.Hash;
using CycloneXmlSerializer = CycloneDX.Xml.Serializer;

public sealed class CycloneDxAnalyzer : Analyzer
{
    private const string DisableCycloneDx = "--disable-cyclonedx";

    private readonly IFileSystem _fileSystem;
    private bool _enabled;

    public override string[] Patterns { get; } = { "**/*.cdx.xml", "**/bom.xml" };
    public override bool Enabled => _enabled;

    public CycloneDxAnalyzer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _enabled = true;
    }

    public override void Initialize(ICommandLineAugmentor cli)
    {
        cli.AddOption<bool>(DisableCycloneDx, "Disables the CycloneDX analyzer", false);
    }

    public override void BeforeAnalysis(AnalysisSettings settings)
    {
        if (settings.Cli.GetOption<bool>(DisableCycloneDx))
        {
            _enabled = false;
        }
    }

    public override bool CanHandle(AnalysisContext context, FilePath path)
    {
        var filename = path.GetFilename();
        return filename.FullPath.EndsWith(".cdx.xml") ||
               filename.FullPath.Equals("bom.xml", StringComparison.Ordinal);
    }

    public override void Analyze(AnalysisContext context, FilePath path)
    {
        var bom = ReadCycloneDxBom(path);
        if (bom == null)
        {
            return;
        }

        // Consider the SBOM main component a root
        context.AddComponent(
            ToBomComponent(
                context,
                bom.Metadata.Component,
                BomComponentKind.Root));

        foreach (var component in bom.Components)
        {
            switch (component.Type)
            {
                case CycloneComponent.Classification.Application:
                    context.AddComponent(ToBomComponent(context, component, BomComponentKind.Application));
                    break;
                case CycloneComponent.Classification.Framework:
                case CycloneComponent.Classification.Library:
                    // TODO: Add new component kinds
                    context.AddComponent(ToBomComponent(context, component, BomComponentKind.Library));
                    break;
                case CycloneComponent.Classification.File:
                    context.AddFile(ToBomFile(component));
                    break;
                case CycloneComponent.Classification.Null:
                case CycloneComponent.Classification.Container:
                case CycloneComponent.Classification.Firmware:
                case CycloneComponent.Classification.Operating_System:
                case CycloneComponent.Classification.Device:
                case CycloneComponent.Classification.Device_Driver:
                case CycloneComponent.Classification.Platform:
                case CycloneComponent.Classification.Machine_Learning_Model:
                case CycloneComponent.Classification.Data:
                    // Ignore
                    break;
                default:
                    throw new InvalidOperationException("Unknown component type");
            }
        }

        foreach (var dependency in bom.Dependencies)
        {
            var origin = context.Graph.Nodes.FirstOrDefault(x => x.Purl == dependency.Ref);
            if (origin == null)
            {
                context.AddWarning("Component is missing")
                    .WithContext("Scope", dependency.Ref);
                continue;
            }

            if (dependency.Dependencies != null)
            {
                foreach (var dep in dependency.Dependencies)
                {
                    var to = context.Graph.Nodes.FirstOrDefault(x => x.Purl == dep.Ref);
                    if (to == null)
                    {
                        context.AddWarning("Component is missing")
                            .WithContext("Scope", dep.Ref);
                        continue;
                    }

                    context.Connect(origin, to);
                }
            }
        }
    }

    private BomComponent ToBomComponent(AnalysisContext context, CycloneComponent component, BomComponentKind kind)
    {
        var bom = new BomComponent(component.Purl, component.Name, component.Version, kind);

        if (component.Licenses?.Count > 1)
        {
            context.AddWarning("Component contains more than one license. This is currently not supported")
                .WithContext("Scope", component.Purl);
        }

        var license = component.Licenses?.FirstOrDefault();
        if (license != null)
        {
            if (license.Expression != null ||
                license.License != null)
            {
                bom.License = new BomLicense
                {
                    Name = license.License?.Name,
                    Expression = license.Expression,
                    Id = license.License?.Id,
                    Url = license.License?.Url,
                };

                if (license.License?.Text is { Encoding: "base64" })
                {
                    bom.License.Text = Base64EncodedText.FromEncoded(license.License.Text.Content);
                }
            }
        }

        return bom;
    }

    private BomFile ToBomFile(CycloneComponent component)
    {
        return new BomFile(component.Name, ToBomHah(component.Hashes.FirstOrDefault()));
    }

    private BomHash ToBomHah(CycloneHash? hash)
    {
        var alg = hash?.Alg switch
        {
            null => BomHashAlgorithm.Unknown,
            CycloneHash.HashAlgorithm.Null => BomHashAlgorithm.Unknown,
            CycloneHash.HashAlgorithm.MD5 => BomHashAlgorithm.MD5,
            CycloneHash.HashAlgorithm.SHA_1 => BomHashAlgorithm.SHA1,
            CycloneHash.HashAlgorithm.SHA_256 => BomHashAlgorithm.SHA256,
            CycloneHash.HashAlgorithm.SHA_384 => BomHashAlgorithm.SHA384,
            CycloneHash.HashAlgorithm.SHA_512 => BomHashAlgorithm.SHA512,
            CycloneHash.HashAlgorithm.SHA3_256 => BomHashAlgorithm.SHA3_256,
            CycloneHash.HashAlgorithm.SHA3_384 => BomHashAlgorithm.SHA3_384,
            CycloneHash.HashAlgorithm.SHA3_512 => BomHashAlgorithm.SHA3_512,
            CycloneHash.HashAlgorithm.BLAKE2b_256 => BomHashAlgorithm.BLAKE2b_256,
            CycloneHash.HashAlgorithm.BLAKE2b_384 => BomHashAlgorithm.BLAKE2b_384,
            CycloneHash.HashAlgorithm.BLAKE2b_512 => BomHashAlgorithm.BLAKE2b_512,
            CycloneHash.HashAlgorithm.BLAKE3 => BomHashAlgorithm.BLAKE3,
            _ => throw new InvalidOperationException("Unknown hash algorithm"),
        };

        return new BomHash(alg, hash?.Content ?? string.Empty);
    }

    private CycloneBom? ReadCycloneDxBom(FilePath path)
    {
        var file = _fileSystem.GetFile(path);
        if (!file.Exists)
        {
            throw new FileNotFoundException("The CycloneDX BOM file could not be found", path.FullPath);
        }

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            return CycloneXmlSerializer.Deserialize(json);
        }
    }
}