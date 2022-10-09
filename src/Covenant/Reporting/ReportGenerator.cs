using Scriban;

namespace Covenant.Reporting;

public sealed class ReportGenerator
{
    public string Generate(Bom bom)
    {
        // Parse template
        var templateHtml = EmbeddedResourceReader.Read("Covenant/Reporting/Templates/Template.html");
        var template = Template.Parse(templateHtml);

        // Render template
        return template.Render(new { bom = new ReportContext(bom) });
    }
}
