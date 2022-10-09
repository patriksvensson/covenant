namespace Covenant.Spdx;

public class SpdxSerializer : BomSerializer
{
    public override string Id { get; } = "spdx";
    public override string Name { get; } = "SPDX";
    public override string Extension { get; } = ".spdx.json";

    public override void Initialize(ICommandLineAugmentor cli)
    {
        cli.AddOption<string>("--namespace", "The SPDX namespace", null);
    }

    public override string? Serialize(Bom bom, BomSerializerSettings settings, ICommandLineResolver cli)
    {
        var @namespace = cli.GetOption<string>("--namespace");

        var document = SpdxConverter.Convert(bom, settings, @namespace);
        return document.Serialize(SpdxDocumentFormat.Json);
    }
}