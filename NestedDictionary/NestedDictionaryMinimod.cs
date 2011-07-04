using System.Collections.Generic;

namespace Minimod.NestedDictionary
{
    /// <summary>
    /// <h1>Minimod.NestedDictionary, Version 0.9.0</h1>
    /// <para>Sometimes you wan't to collect or cache some data in lists by key, or even nested dictionaries. </para>
    /// <para>Then, for each new key, you have to instanciate the list or nested dictionary, before you can put it in.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class NestedDictionaryMinimod
    {
        /// <summary>
        /// Ensures a <typeparamref name="TCol"/> for <typeparamref name="TKey"/>, and adds all <paramref name="items"/> to it.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key (usually inferred).</typeparam>
        /// <typeparam name="TCol">The concrete collection type (usually inferred).</typeparam>
        /// <typeparam name="TItem">The type of the list-items (usually inferred).</typeparam>
        /// <param name="dict">The dictionary to operate on.</param>
        /// <param name="key">The key, where to enlist the items.</param>
        /// <param name="items">The values to add to the list per key.</param>
        public static void AddToCollection<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dict, TKey key, IEnumerable<TItem> items)
          where TCol : ICollection<TItem>, new()
        {
            TCol col = dict.EnsureValueFor(key);
            foreach (var item in items)
            {
                col.Add(item);
            }
        }

        /// <summary>
        /// Ensures a <typeparamref name="TCol"/> for <typeparamref name="TKey"/>, and add the <paramref name="item"/> to it.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key (usually inferred).</typeparam>
        /// <typeparam name="TCol">The concrete collection type (usually inferred).</typeparam>
        /// <typeparam name="TItem">The type of the list-items (usually inferred).</typeparam>
        /// <param name="dict">The dictionary to operate on.</param>
        /// <param name="key">The key, where to enlist the items.</param>
        /// <param name="item">The item to add to the list for key.</param>
        public static void AddToCollection<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dict, TKey key, TItem item)
          where TCol : ICollection<TItem>, new()
        {
            dict.EnsureValueFor(key).Add(item);
        }

        /// <summary>
        /// If necessary, initializes the dictionary for <paramref name="key"/>, whereever <typeparamref name="TValue"/> has a default constructor.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
        /// <param name="dict">The dictionary to operate on.</param>
        /// <param name="key">The key for which to ensure the value.</param>
        /// <returns>The new or existing value for dict[key].</returns>
        public static TValue EnsureValueFor<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue list;
            if (!dict.TryGetValue(key, out list))
            {
                dict.Add(key, list = new TValue());
            }
            return list;
        }
    }
}
