using System.Text;
using System.Xml.Linq;
using Covenant.Core;
using Covenant.Core.Model;
using Covenant.CycloneDx;
using Shouldly;

namespace Covenant.Tests;

public class CycloneDxSerializerTests
{
    [Fact]
    public void ShouldSerializeValidXmlWhenAllBomLicenseMembersAreNotNull()
    {
        var bom = new Bom("test", "1.0")
        {
            Components = new List<BomComponent>
            {
                new BomComponent("purl", "name", "version", BomComponentKind.Library)
                {
                    License = new BomLicense { Id = "MIT", Expression = "MIT", Name = "MIT", Text = Base64EncodedText.Encode("MIT License"), Url = "https://spdx.org/licenses/MIT.html" },
                },
            },
            Dependencies = new List<BomDependency>(),
        };

        var serializer = new CycloneDxSerializer();

        var cycloneDxBom = serializer.Serialize(bom, new BomSerializerSettings(), new DefaultValueCommandLineResolver());
        cycloneDxBom.ShouldNotBeNull();
        var document = ParseBom(cycloneDxBom);

        var validator = new CycloneDxValidator("./schemas/CycloneDX");
        validator.Validate(document);
    }

    private XDocument ParseBom(string text)
    {
        text = RemoveByteOrderMark(text);
        var document = XDocument.Parse(text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        return document;
    }

    private string RemoveByteOrderMark(string text)
    {
        var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        if (text.StartsWith(byteOrderMarkUtf8))
        {
            text = text.Remove(0, byteOrderMarkUtf8.Length);
        }

        return text;
    }

    public class DefaultValueCommandLineResolver : ICommandLineResolver
    {
        public T? GetOption<T>(string alias) => default(T);
    }
}