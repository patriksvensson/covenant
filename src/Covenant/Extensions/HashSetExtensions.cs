namespace Covenant;

public static class HashSetExtensions
{
    public static void AddRange<T>(this HashSet<T> source, IEnumerable<T>? items)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (items != null)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }
    }
}
