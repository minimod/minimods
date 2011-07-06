using System;
using System.Collections.Generic;

namespace Minimod.Flattener
{
    /// <summary>
    /// Minimod.Flattener, Version 0.0.1
    /// <para>A small hierarchy flattener for IEnumerable´1.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class FlattenerMinimod
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