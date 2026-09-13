using Scriban;
using Scriban.Runtime;

namespace Covenant.Reporting;

public sealed class ReportGenerator
{
    public string Generate(Bom bom)
    {
        // Parse template
        var templateHtml = EmbeddedResourceReader.Read("Covenant/Reporting/Templates/Template.html");
        var template = Template.Parse(templateHtml);

        // Render template. Loop limit disabled: large SBOMs routinely exceed Scriban's default
        // 1000-iteration cap, and Template.html is a trusted, fixed resource, not user input.
        var templateContext = new TemplateContext { LoopLimit = 0 };
        templateContext.PushGlobal(new ScriptObject { ["bom"] = new ReportContext(bom) });

        return template.Render(templateContext);
    }
}
