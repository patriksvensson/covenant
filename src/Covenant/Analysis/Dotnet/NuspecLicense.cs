namespace Covenant.Analysis.Dotnet;

public sealed class NuspecLicense
{
    public string? Id { get; set; }
    public string? Url { get; set; }
    public string? Text { get; set; }
    public string? Copyright { get; set; }

    public bool IsValid => Id != null || Url != null || Text != null;

    public BomLicense? ConvertToBomLicense()
    {
        if (!IsValid)
        {
            return null;
        }

        var id = Id;
        var name = default(string);
        var expression = default(string);
        var url = Url;
        var text = Text;

        if (!string.IsNullOrWhiteSpace(id) && SpdxLicense.TryGetById(id, out var spdxLicense))
        {
            id = spdxLicense.Id;
            name = spdxLicense.Name;
        }
        else
        {
            if (id != null && SpdxExpression.IsValidExpression(id, SpdxLicenseOptions.Relaxed))
            {
                expression = id;
                id = null;
            }
        }

        return new BomLicense
        {
            Id = id,
            Name = name,
            Expression = expression,
            Url = url,
            Text = Base64EncodedText.Encode(text),
        };
    }
}