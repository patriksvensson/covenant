namespace Covenant.CycloneDx;

public class CycloneDxSerializer : BomSerializer
{
    public override string Id { get; } = "cyclonedx";
    public override string Name { get; } = "CycloneDX";
    public override string Extension { get; } = ".cdx.xml";

    public override string? Serialize(Bom bom, BomSerializerSettings settings, ICommandLineResolver cli)
    {
        var cycloneBom = CycloneDxConverter.Convert(bom, settings);
        return CycloneXmlSerializer.Serialize(cycloneBom);
    }
}