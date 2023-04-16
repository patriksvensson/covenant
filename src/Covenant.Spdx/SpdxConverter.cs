using System.ComponentModel;

namespace Covenant.Spdx;

internal static class SpdxConverter
{
    public static CovenantSpdxDocument Convert(Bom bom, BomSerializerSettings settings, string? @namespace)
    {
        var name = settings.Name ?? "Unknown";
        var version = settings.Version ?? "0.0.0";

        var document = new CovenantSpdxDocument
        {
            SpdxId = "SPDXRef-DOCUMENT",
            DocumentName = name,
            DocumentNamespace = (@namespace ?? $"http://spdx.org/spdxdocs/{name}-{settings.Version}") + $"-{Guid.NewGuid()}",
            CreationInfo = new SpdxCreationInfo
            {
                Created = DateTimeOffset.Now,
                Creators = new List<string>
                {
                    "Tool: Covenant",
                },
            },
        };

        // Add metadata
        if (bom.Metadata.Count > 0)
        {
            document.DocumentComment = string.Join(", ", bom.Metadata.Select(m => $"{m.Key}={m.Value}"));
        }

        // Add non-SPDX licenses
        var counter = 1;
        foreach (var component in bom.Components)
        {
            if (component.License != null)
            {
                var license = document.AddLicense(component.License);
                if (license != null)
                {
                    license.LicenseId = "LicenseRef-" + counter;
                    counter++;
                }
            }
        }

        // Add files
        foreach (var file in bom.Files)
        {
            document.Files.Add(new SpdxFile
            {
                SpdxId = $"SPDXRef-{file.Path.ToSpdxId()}",
                Filename = file.Path,
                LicenseConcluded = file.License?.Id ?? "NOASSERTION",
                Checksums =
                {
                    new SpdxChecksum
                    {
                        Algorithm = file.Hash.Algorithm.ToString(),
                        Value = file.Hash.Content,
                    },
                },
            });
        }

        // Add relationship between document and files.
        foreach (var file in bom.Files)
        {
            document.Relationships.Add(new SpdxRelationship
            {
                Identifier = document.SpdxId,
                Type = "DESCRIBES",
                RelatedIdentifier = $"SPDXRef-{file.Path.ToSpdxId()}",
            });
        }

        // Add components
        foreach (var component in bom.Components)
        {
            var license = document.FindLicense(component.License);

            var package = new CovenantSpdxPackage(component)
            {
                SpdxId = $"SPDXRef-{component.Name.ToSpdxId()}-{component.UUID}",
                PackageName = component.Name,
                VersionInfo = component.Version,
                PackageDownloadLocation = "NOASSERTION",
                LicenseConcluded = component.License?.Id ?? license?.LicenseId ?? "NOASSERTION",
                LicenseDeclared = component.License?.Id ?? license?.LicenseId ?? "NOASSERTION",
                CopyrightText = component.Copyright ?? "NOASSERTION",
            };

            if (component.IsRoot)
            {
                package.VersionInfo = settings.Version ?? component.Version;
                package.CopyrightText = "NOASSERTION";
            }

            if (component.Kind == BomComponentKind.Library)
            {
                package.ExternalReferences = new List<SpdxExternalReference>
                {
                    new SpdxExternalReference
                    {
                        Category = "PACKAGE-MANAGER",
                        Type = "purl",
                        Locator = component.Purl,
                    },
                };
            }

            if (component.Hash != null)
            {
                package.Checksums.Add(new SpdxChecksum
                {
                    Algorithm = component.Hash.Algorithm.ToString(),
                    Value = component.Hash.Content,
                });
            }

            document.Packages.Add(package);
        }

        // Add relationship between document and root components.
        foreach (var component in bom.Components.Where(c => c.IsRoot))
        {
            document.Relationships.Add(new SpdxRelationship
            {
                Identifier = document.SpdxId,
                Type = "CONTAINS",
                RelatedIdentifier = $"SPDXRef-{component.Name.ToSpdxId()}-{component.UUID}",
            });
        }

        // Add dependencies
        foreach (var bomDependency in bom.Dependencies)
        {
            var bomComponent = document.FindPackageByPurl(bomDependency.Purl);
            if (bomComponent == null)
            {
                throw new InvalidOperationException("Could not find component dependency");
            }

            foreach (var dependency in bomDependency.Dependencies)
            {
                var dependencyComponent = document.FindPackageByPurl(dependency);
                if (dependencyComponent == null)
                {
                    throw new InvalidOperationException("Could not find dependency");
                }

                document.Relationships.Add(new SpdxRelationship
                {
                    Identifier = bomComponent.SpdxId,
                    Type = "DEPENDS_ON",
                    RelatedIdentifier = dependencyComponent.SpdxId,
                });
            }
        }

        return document;
    }
}