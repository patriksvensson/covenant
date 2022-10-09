namespace Covenant.Core;

public static class BomReader
{
    public static Bom? Read(string json)
    {
        return JsonConvert.DeserializeObject<Bom>(json);
    }
}