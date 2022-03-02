using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebJobs.Extensions.Ftp.Extensions;

internal static class LinqExtensions
{
    public static IEnumerable<IEnumerable<T>> GetBatches<T>(this IEnumerable<T> source, int batchSize)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var currentPage = new List<T>(batchSize)
            {
                enumerator.Current
            };

            while (currentPage.Count < batchSize && enumerator.MoveNext())
            {
                currentPage.Add(enumerator.Current);
            }

            yield return new ReadOnlyCollection<T>(currentPage);
        }
    }
}