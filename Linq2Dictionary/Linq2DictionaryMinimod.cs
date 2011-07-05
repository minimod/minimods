using System;
using System.Collections.Generic;
using System.Linq;

namespace Minimod.Linq2Dictionary
{
    /// <summary>
    /// <h1>Minimod.Linq2Dictionary, Version 0.9.0, Copyright © Lars Corneliussen 2011</h1>
    /// <para>Extends the Dictionary with some linquish Methods like Union, (more to come).</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class Linq2DictionaryMinimod
    {
        /// <summary>
        /// Unions the key-value-pairs of two dictionaries into one.
        /// </summary>
        public static IDictionary<TKey, TCol> Union<TKey, TCol>(this IDictionary<TKey, TCol> first, IDictionary<TKey, TCol> second)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");

            if (first.Count == 0 && second.Count == 0)
                return new Dictionary<TKey, TCol>(0);

            if (first.Count == 0)
                return second.ToDictionary(); // actually a clone

            if (second.Count == 0)
                return first.ToDictionary(); // actually a clone

            return first.ToArray().Union(second).ToDictionary();
        }

        /// <summary>
        /// Creates a dictionary from enumerable key-value-pairs.
        /// </summary>
        public static IDictionary<TKey, TCol> ToDictionary<TKey, TCol>(this IEnumerable<KeyValuePair<TKey, TCol>> pairs)
        {
            return pairs.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
