namespace Covenant;

public static class ListExtensions
{
    public static IReadOnlyList<T> Merge<T>(this IReadOnlyList<T>? source, IReadOnlyList<T> other)
    {
        if (source == null)
        {
            return other;
        }

        return source.Concat(other).ToList();
    }

    public static void AddIfNotNull<T>(this List<T> list, T? item)
        where T : class
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (item != null)
        {
            list.Add(item);
        }
    }
}
