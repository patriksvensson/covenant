using System.Xml.Linq;
using Shouldly;

namespace Covenant.Tests;

[ShouldlyMethods]
public static class CycloneDxShouldlyExtensions
{
    private static readonly XNamespace _ns = "http://cyclonedx.org/schema/bom/1.5";

    public static XElement ShouldHaveSingleComponent(this XDocument document, string? customMessage = null)
    {
        var components = document.Descendants(_ns + "component").ToList();

        if (components.Count != 1)
        {
            throw new ShouldAssertException(new ExpectedShouldlyMessage(document, customMessage).ToString());
        }

        return components.First();
    }

    public static XElement ShouldHaveOneLicenseExpression(this XElement component, string? customMessage = null)
    {
        var licenses = component.Element(_ns + "licenses");
        var expressions = licenses?.Descendants(_ns + "expression").ToList();

        if (licenses == null || expressions == null || expressions.Count != 1)
        {
            throw new ShouldAssertException(new ExpectedShouldlyMessage(component, customMessage).ToString());
        }

        return expressions.First();
    }
}