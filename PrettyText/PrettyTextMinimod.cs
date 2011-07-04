using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minimod.PrettyText
{
    /// <summary>
    /// Minimod.PrettyText, Version 0.9.6
    /// <para>A minimod with string extensions, helping whereever you have to shape text to fit into a box.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class PrettyTextMinimod
    {
        /// <summary>
        /// Fits the text into <paramref name="length"/>, appending <value>...</value> if it is too long.
        /// </summary>
        public static string ShortenTo(this string text, int length)
        {
            return text.Length > length ? text.Substring(0, length - 3) + "..." : text;
        }

        /// <summary>
        /// Splits into lines by using <see cref="Environment.NewLine"/>.
        /// </summary>
        public static IEnumerable<string> SplitLines(this string input)
        {
            return input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        /// <summary>
        /// Join lines by using <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string JoinLines(this IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>
        /// Wraps lines exceeding a certain margin. For each line, the indentation is maintained.
        /// </summary>
        /// <example>
        /// <code>
        /// <pre>
        /// Some long line here
        ///   Another indented long line here withalongerthan15word
        /// </pre>
        /// </code>
        /// wrapped at 15 will produce:
        /// <code>
        /// <pre>
        /// Some long line 
        /// here
        ///   Another 
        ///   indented long 
        ///   line here 
        ///   withalongerth
        ///   an15word
        /// </pre>
        /// </code>
        /// </example>
        /// <remarks>
        /// based on: http://blueonionsoftware.com/blog.aspx?p=6091173d-6bdb-498c-9d57-c0da43319839
        /// </remarks>
        public static IEnumerable<string> WrapAt(this string input, int margin)
        {
            var resultLines = new List<string>();

            foreach (var line in SplitLines(input))
            {
                var lines = new List<string>();
                var leadingSpaces = Regex.Match(line, "^\\s*").Value.Length;

                var sublineMargin = margin - leadingSpaces;
                var text = line.Substring(leadingSpaces).TrimEnd();

                int start = 0, end;

                while ((end = start + sublineMargin) < text.Length)
                {
                    // walk back; is there a space in the line?
                    while (text[end] != ' ' && end > start)
                        end -= 1;

                    // ok, we found a space
                    if (end != start)
                    {
                        lines.Add(text.Substring(start, end - start));
                        start = end + 1; // +1 means jump over the space
                    }
                    // no space? then crop hardly
                    else
                    {
                        end = start + sublineMargin;
                        lines.Add(text.Substring(start, end - start));
                        start = end;
                    }
                }

                if (start < text.Length)
                    lines.Add(text.Substring(start));

                resultLines.AddRange(lines.IndentBy(leadingSpaces));
            }

            return resultLines;
        }

        /// <summary>
        /// Indents each line by specified number of spaces.
        /// </summary>
        public static IEnumerable<string> IndentBy(this IEnumerable<string> lines, int leadingSpaces)
        {
            return lines.Select(_ => new string(' ', leadingSpaces) + _);
        }
    }
}
