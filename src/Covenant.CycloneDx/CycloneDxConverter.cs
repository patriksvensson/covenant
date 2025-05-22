using Bom = Covenant.Core.Model.Bom;

namespace Covenant.CycloneDx;

internal static class CycloneDxConverter
{
    public static CycloneBom Convert(Bom bom, BomSerializerSettings settings)
    {
        var name = settings.Name;
        var version = settings.Version;

        // Get the main component
        var root = default(BomComponent);
        if (!string.IsNullOrWhiteSpace(name))
        {
            // TODO: Need to be an existing component?
            root = new BomComponent(name, name, version ?? "0.0.0", BomComponentKind.Root);
        }

        var result = new CycloneBom
        {
            Components = new List<CycloneComponent>(),
            Dependencies = new List<CycloneDependency>(),
            Metadata = new CycloneMetadata
            {
                Component = ConvertComponent(root),
                Tools = new CycloneToolChoices
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    Tools = new List<CycloneTool>
                    {
                        new CycloneTool
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            Name = "Covenant",
                            Vendor = bom.ToolVendor,
                            Version = bom.ToolVersion,
                        },
                    },
                },
                Properties = new List<CycloneProperty>
                {
                    new CycloneProperty
                    {
                        Name = "timestamp",
                        Value = DateTime.UtcNow.ToString("o"),
                    },
                    new CycloneProperty
                    {
                        Name = "components",
                        Value = bom.Components.Count.ToString(),
                    },
                    new CycloneProperty
                    {
                        Name = "dependencies",
                        Value = bom.Dependencies.Count.ToString(),
                    },
                },
            },
        };

        var groups = new HashSet<string>(bom.Components.SelectMany(x => x.Groups), StringComparer.Ordinal);
        if (groups.Count > 0)
        {
            result.Metadata.Properties.Add(new CycloneProperty
            {
                Name = "groups",
                Value = string.Join(";", groups),
            });
        }

        if (bom.Metadata.Count > 0)
        {
            foreach (var metadata in bom.Metadata)
            {
                result.Metadata.Properties.Add(new CycloneProperty
                {
                    Name = metadata.Key,
                    Value = metadata.Value,
                });
            }
        }

        foreach (var file in bom.Files)
        {
            var component = new CycloneComponent
            {
                Type = CycloneComponent.Classification.File,
                BomRef = file.Path,
                Name = file.Path,
                Hashes = new List<CycloneHash>
                {
                    ConvertHash(file.Hash),
                },
            };

            if (file.License != null)
            {
                component.Licenses = new List<CycloneLicenseChoice>
                {
                    ConvertLicense(file.License),
                };
            }

            result.Components.Add(component);
        }

        foreach (var component in bom.Components)
        {
            result.Components.Add(ConvertComponent(component));
        }

        foreach (var dependency in bom.Dependencies)
        {
            result.Dependencies.Add(ConvertDependency(dependency));
        }

        return result;
    }

    private static CycloneComponent? ConvertComponent(BomComponent? component)
    {
        if (component == null)
        {
            return null;
        }

        var result = new CycloneComponent
        {
            BomRef = component.Purl,
            Purl = component.Purl,
            Name = component.Name,
            Version = component.Version,
            Type = ConvertKind(component.Kind),
            Properties = new List<CycloneProperty>(),
        };

        if (component.IsRoot)
        {
            result.Properties.Add(new CycloneProperty
            {
                Name = "root",
                Value = "true",
            });
        }

        if (component.Groups.Count > 0)
        {
            result.Properties.Add(new CycloneProperty
            {
                Name = "groups",
                Value = string.Join(";", component.Groups),
            });
        }

        if (component.Source != null)
        {
            result.Properties.Add(new CycloneProperty
            {
                Name = "source",
                Value = component.Source,
            });
        }

        if (component.Hash != null)
        {
            result.Hashes = new List<CycloneHash>
            {
                ConvertHash(component.Hash),
            };
        }

        if (component.License != null)
        {
            result.Licenses = new List<CycloneLicenseChoice>
            {
                ConvertLicense(component.License),
            };
        }

        return result;
    }

    private static CycloneDependency? ConvertDependency(BomDependency dependency)
    {
        return new CycloneDependency
        {
            Ref = dependency.Purl,
            Dependencies = dependency.Dependencies.ConvertAll(d => new CycloneDependency { Ref = d }),
        };
    }

    private static CycloneComponent.Classification ConvertKind(BomComponentKind kind)
    {
        return kind switch
        {
            BomComponentKind.Root => CycloneComponent.Classification.Application,
            BomComponentKind.Library => CycloneComponent.Classification.Library,
            BomComponentKind.Application => CycloneComponent.Classification.Application,
            _ => throw new NotSupportedException("Unknown component kind"),
        };
    }

    private static CycloneHash ConvertHash(BomHash hash)
    {
        return new CycloneHash
        {
            Alg = hash.Algorithm switch
            {
                BomHashAlgorithm.Unknown => CycloneHash.HashAlgorithm.Null,
                BomHashAlgorithm.SHA1 => CycloneHash.HashAlgorithm.SHA_1,
                BomHashAlgorithm.SHA256 => CycloneHash.HashAlgorithm.SHA_256,
                BomHashAlgorithm.SHA512 => CycloneHash.HashAlgorithm.SHA3_512,
                _ => throw new NotSupportedException("Unknown hash algorithm"),
            },
            Content = hash.Content,
        };
    }

    private static CycloneLicenseChoice ConvertLicense(BomLicense license)
    {
        var choice = new CycloneLicenseChoice();

        if (license.Expression != null)
        {
            choice.Expression = license.Expression;
            return choice;
        }

        if (license.Id != null)
        {
            choice.License ??= new CycloneLicense();
            choice.License.Id = license.Id;
        }

        if (license.Url != null)
        {
            choice.License ??= new CycloneLicense();
            choice.License.Url = license.Url;
        }

        if (license.Name != null)
        {
            choice.License ??= new CycloneLicense();
            choice.License.Name = license.Name;
        }

        if (license.Text != null)
        {
            choice.License ??= new CycloneLicense();
            choice.License.Text = new CycloneAttachedText
            {
                Content = license.Text.Encoded,
                ContentType = "text/plain",
                Encoding = "base64",
            };
        }

        return choice;
    }
}