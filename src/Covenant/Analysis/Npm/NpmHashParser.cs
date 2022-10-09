namespace Covenant.Analysis.Npm;

// Parses "Subresource Integrity"
// See https://w3c.github.io/webappsec-subresource-integrity/
internal static class NpmHashParser
{
    public static BomHash? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var index = text.IndexOf('-');
        if (index == -1)
        {
            // Unknown hash format
            return null;
        }

        var algorithm = text[..index].ToLowerInvariant() switch
        {
            "sha1" => BomHashAlgorithm.SHA1,
            "sha512" => BomHashAlgorithm.SHA512,
            _ => BomHashAlgorithm.Unknown,
        };

        // TODO: Always base64 encoded?
        return new BomHash(
            algorithm,
            BitConverter.ToString(
                Convert.FromBase64String(
                    text.Substring(index + 1, text.Length - index - 1)))
            .Replace("-", string.Empty));
    }
}
