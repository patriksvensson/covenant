namespace Covenant.Spdx;

public static class StringExtensions
{
    public static string? ToSpdxId(this string? text)
    {
        if (text == null)
        {
            return null;
        }

        var index = 0;
        var array = new char[text.Length];
        foreach (var c in text)
        {
            if (char.IsLetter(c) || char.IsDigit(c) ||
               c == '.' || c == '-')
            {
                array[index] = c;
                index++;
            }
        }

        return new string(array.AsSpan(0, index));
    }
}