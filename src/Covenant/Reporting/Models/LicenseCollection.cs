using System.Collections;

namespace Covenant.Reporting;

public class LicenseCollection : LicenseCount, IReadOnlyDictionary<string, int>
{
    private readonly Dictionary<string, int> _lookup;

    public IEnumerable<string> Keys => _lookup.Keys;
    public IEnumerable<int> Values => _lookup.Values;

    public int this[string id]
    {
        get
        {
            _lookup.TryGetValue(id, out var value);
            return value;
        }
    }

    public LicenseCollection()
    {
        _lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    public void Add(string id)
    {
        if (_lookup.TryAdd(id, 1))
        {
            // New item
            AddDistinct();
        }
        else
        {
            // Existing one
            AddExisting();
            _lookup[id]++;
        }
    }

    public bool ContainsKey(string key)
    {
        return _lookup.ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out int value)
    {
        return _lookup.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
    {
        return _lookup.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
