namespace BisleriumBloggers.Helper;

public static class ExtensionMethod
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var random = new Random();

        var n = list.Count;

        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
