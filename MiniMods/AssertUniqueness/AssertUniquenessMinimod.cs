using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minimod.AssertUniqueness
{
    internal static class AssertUniquenessMinimod
    {
        public static bool AssertIsUnique<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, Action<IDictionary<TKey, T[]>> handleExceptions)
        {
            var duplicates = enumerable.GroupBy(keySelector).Where(_ => _.Count() > 1).ToArray();
            if (duplicates.Length > 0)
            {
                handleExceptions(duplicates.ToDictionary(_ => _.Key, _ => _.ToArray()));
                return false;
            }
            return true;
        }
    }
}
