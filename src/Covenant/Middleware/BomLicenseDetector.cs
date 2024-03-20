using System.Text.RegularExpressions;

namespace Covenant.Middleware;

// TODO: Make this more sophisticated :)
public static class BomLicenseDetector
{
    private static readonly Regex _regex = new(@"\s+");

    private static readonly Dictionary<string, string> _licenses = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Apache License, Version 1.0", "Apache-1.0" },
        { "Apache License, Version 1.1", "Apache-1.1" },
        { "Apache License, Version 2.0", "Apache-2.0" },
        { "Apache License Version 1.0", "Apache-1.0" },
        { "Apache License Version 1.1", "Apache-1.1" },
        { "Apache License Version 2.0", "Apache-2.0" },
        { "The MIT License (MIT)", "MIT" },
        { "MIT", "MIT" },
        { "GNU LESSER GENERAL PUBLIC LICENSE 2.1", "LGPL-2.1" },
    };

    private static readonly Dictionary<string, string> _urls = new(StringComparer.OrdinalIgnoreCase)
    {
        { "https://go.microsoft.com/fwlink/?linkid=869050", "MIT" },
        { "https://opensource.org/licenses/mit", "MIT" },
        { "https://raw.githubusercontent.com/xunit/xunit/master/license.txt", "Apache-2.0" },
        { "https://raw.githubusercontent.com/xunit/xunit.analyzers/master/LICENSE", "Apache-2.0" },
        { "https://github.com/dotnet/standard/blob/master/LICENSE.TXT", "MIT" },
        { "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT", "MIT" },
        { "https://opensource.org/licenses/mpl-2.0", "MPL-2.0" },
        { "https://www.gnu.org/licenses/old-licenses/lgpl-2.1.en.html", "LGPL-2.1" },
    };

    private static readonly Dictionary<string, string> _hashes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "60E7688A88B5D124B839853C3836CE6E", "MIT" },
    };

    public static bool TryDetectLicense(string? text, string? hash, string? url, out string? id, out string? expression)
    {
        var licenses = new HashSet<string>();

        // Known hash?
        if (hash != null && _hashes.TryGetValue(hash, out var hashLicense))
        {
            licenses.Add(hashLicense);
        }

        // Known URL?
        if (url != null && _urls.TryGetValue(url, out var urlLicense))
        {
            licenses.Add(urlLicense);
        }

        if (text != null)
        {
            text = ReplaceWhitespace(text, " ");
            foreach (var (query, license) in _licenses)
            {
                if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    licenses.Add(license);
                }
            }
        }

        if (licenses.Count == 1)
        {
            id = licenses.First();
            expression = null;
            return true;
        }
        else if (licenses.Count > 1)
        {
            id = null;
            expression = string.Join(" OR ", licenses);
            return true;
        }
        else
        {
            id = null;
            expression = null;
            return false;
        }
    }

    private static string ReplaceWhitespace(string input, string replacement)
    {
        return _regex.Replace(input, replacement);
    }
}