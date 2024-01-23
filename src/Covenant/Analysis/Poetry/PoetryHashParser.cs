namespace Covenant.Analysis.Npm;

internal static class PoetryHashParser
{
    public static BomHash? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var index = text.IndexOf(':');
        if (index == -1)
        {
            // Unknown hash format
            return null;
        }

        var algorithm = text[..index].ToLowerInvariant() switch
        {
            "sha256" => BomHashAlgorithm.SHA256,
            _ => BomHashAlgorithm.Unknown,
        };

        // TODO: Is this right?
        return new BomHash(
            algorithm,
            text.Substring(index + 1, text.Length - index - 1));
    }
}
