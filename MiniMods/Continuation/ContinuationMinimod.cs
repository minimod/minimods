using System;

namespace Minimod.Continuation
{
    /// <summary>
    /// Minimod.Continuation, Version 0.0.1
    /// <para>A minimod for easy to use continuation pattern in CSharp.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class ContinuationExtentions
    {
        public static T Continue<T>(this T that, Func<T, T> continuation)
        {
            return continuation(that);
        }

        public static TR Continue<T, TR>(this T that, Func<T, T> continuation, Func<T, TR> selector)
        {
            return selector(continuation(that));
        }

        public static T Continue<T>(this T that, Func<T, bool> condition, Func<T, T> continuation)
        {
            return condition(that) ? continuation(that) : that;
        }

        public static TR Continue<T, TR>(this T that, Func<T, bool> condition, Func<T, T> continuation, Func<T, TR> selector)
        {
            return selector(condition(that) ? continuation(that) : that);
        }
    }
}