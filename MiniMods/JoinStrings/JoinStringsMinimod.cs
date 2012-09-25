using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minimod.JoinStrings
{
    /// <summary>
    /// <h1>Minimod.JoinStrings, Version 1.0.0, Copyright © Lars Corneliussen 2011</h1>
    /// <para>A minimod for fluently joining Enumerables using String.Join(..).</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class JoinStringsMinimod
    {
        public static string JoinStringsWith(this IEnumerable values, string separator)
        {
            return string.Join(separator, values.Cast<object>().Select(_ => _.ToString()).ToArray());
        }

        public static string JoinStringsWith(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values.ToArray());
        }
    }
}
