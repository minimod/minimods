using System;
using System.Collections;

namespace Minimod.FluentNullOrEmpty
{
    /// <summary>
    /// <para>Minimod.FluentNullOrEmpty, Version 0.9.1</para>
    /// <para>A minimod for fluently checking if strings or 
    /// collections are either null and/or empty</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not 
    /// use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class FluentNullOrEmptyExtentions
    {
        public static bool IsNullOrEmpty(this string that)
        {
            return String.IsNullOrEmpty(that);
        }

        public static bool IsNullOrEmpty(this ICollection that)
        {
            return that == null || that.Count == 0;
        }
    }
}