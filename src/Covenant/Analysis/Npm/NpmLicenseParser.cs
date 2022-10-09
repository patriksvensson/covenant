namespace Covenant.Analysis.Npm;

internal static class NpmLicenseParser
{
    public static BomLicense? Parse(object? obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is string text)
        {
            return Parse(text);
        }

        if (obj is JObject jsonObject)
        {
            var id = jsonObject.Value<string>("type");
            var url = jsonObject.Value<string>("url");

            return new BomLicense
            {
                Id = id,
                Url = url,
            };
        }

        return null;
    }

    private static BomLicense? Parse(string? license)
    {
        if (license == null)
        {
            return null;
        }

        if (SpdxLicense.TryGetById(license, out var spdxLicense))
        {
            return new BomLicense
            {
                Id = spdxLicense.Id,
                Expression = spdxLicense.Id,
                Name = spdxLicense.Name,
            };
        }
        else if (Uri.TryCreate(license, UriKind.Absolute, out var uri))
        {
            return new BomLicense
            {
                Url = uri.AbsoluteUri,
            };
        }
        else if (SpdxExpression.IsValidExpression(license, SpdxLicenseOptions.Relaxed))
        {
            return new BomLicense
            {
                Expression = license,
            };
        }

        return new BomLicense
        {
            Id = license,
            Name = "Unknown",
        };
    }
}
