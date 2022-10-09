using System.Security.Cryptography;

namespace Covenant.Core;

public sealed class Base64EncodedText
{
    public string Hash { get; }
    public string Decoded { get; }
    public string Encoded { get; }

    [JsonConstructor]
    private Base64EncodedText(string decoded, string encoded)
    {
        var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(decoded));
        Hash = string.Concat(bytes.Select(x => x.ToString("X2")));

        Decoded = decoded ?? throw new ArgumentNullException(nameof(decoded));
        Encoded = encoded ?? throw new ArgumentNullException(nameof(encoded));
    }

    public static Base64EncodedText? Encode(string? text)
    {
        if (text == null)
        {
            return null;
        }

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        return new Base64EncodedText(text, encoded);
    }
}