using System;
using System.Collections.Generic;

namespace Minimod.LinqExtentions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> flattenBy)
        {
            if (source == null)
                yield break;

            foreach (var value in source)
            {
                yield return value;
                foreach (var child in flattenBy(value).Flatten<T>(flattenBy))
                    yield return child;
            }
        }
    }
}