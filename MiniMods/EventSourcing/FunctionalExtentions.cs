using System;
using System.Collections.Generic;

namespace Minimod.EventSourcing
{
    public static class FunctionalExtentions
    {
        public static Func<TR> Memoize<TR>(this Func<TR> f)
        {
            var value = default(TR);
            var hasValue = false;
            return () =>
                       {
                           if (!hasValue)
                           {
                               hasValue = true;
                               value = f();
                           }
                           return value;
                       };
        }

        public static Func<T, TR> Memoize<T, TR>(this Func<T, TR> f)
        {
            var map = new Dictionary<T, TR>();
            return a =>
                       {
                           TR value;
                           if (map.TryGetValue(a, out value))
                               return value;
                           value = f(a);
                           map.Add(a, value);
                           return value;
                       };
        }
    }
}