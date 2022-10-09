namespace Covenant.Core;

public static class BomWriter
{
    public static string? Write(Bom bom)
    {
        return JsonConvert.SerializeObject(bom, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ShouldSerializeContractResolver.Instance,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
            },
        });
    }
}