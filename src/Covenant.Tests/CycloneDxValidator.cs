using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Covenant.Tests;

public class CycloneDxValidator
{
    private readonly XmlSchemaSet _schemaSet;

    public CycloneDxValidator(string schemasPath)
    {
        _schemaSet = LoadSchemasFromDirectory(schemasPath);
    }

    public void Validate(XDocument document)
    {
        var builder = new StringBuilder();
        var hasErrors = false;

        ThrowIfSchemaIsMissing(_schemaSet, document);

        document.Validate(_schemaSet, (_, e) =>
        {
            builder.AppendLine($"(Line: {e.Exception.LineNumber} Column: {e.Exception.LinePosition}) {e.Severity}: {e.Message}");
            hasErrors = true;
        });

        if (hasErrors)
        {
            var message = "CycloneDX BOM failed schema validation." + Environment.NewLine + Environment.NewLine + builder;
            throw new XmlException(message);
        }
    }

    private static void ThrowIfSchemaIsMissing(XmlSchemaSet schemaSet, XDocument document)
    {
        var documentNamespace = document.Root?.Name.Namespace;

        if (documentNamespace == null || string.IsNullOrWhiteSpace(documentNamespace.NamespaceName))
        {
            throw new ArgumentException("XDocument missing namespace on root element", nameof(document));
        }

        if (!schemaSet.Contains(documentNamespace.NamespaceName))
        {
            throw new ArgumentException("XDocument root element has an unknown schema: " + documentNamespace, nameof(document));
        }
    }

    private static XmlSchemaSet LoadSchemasFromDirectory(string schemaDirectory)
    {
        var schemaSet = new XmlSchemaSet();

        foreach (var file in Directory.EnumerateFiles(schemaDirectory, "*.xsd"))
        {
            using var stream = File.OpenRead(file);
            using var reader = XmlReader.Create(stream);
            schemaSet.Add(null, reader);
        }

        schemaSet.Compile();
        return schemaSet;
    }
}