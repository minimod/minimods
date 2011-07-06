﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Minimod.PrettyTypeSignatures
{
    /// <summary>
    /// Minimod.PrettyTypeSignatures, Version 0.9
    /// <para>A minimod with reflection extensions, printing nice type and type member names.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class PrettyTypeSignaturesMinimod
    {

        public static string GetPrettyName(this Type type)
        {
            return getDisplayName(type, null);
        }

        public static string GetPrettyName(this MethodBase method)
        {
            if (method == null) throw new ArgumentNullException("method");

            var sb = new StringBuilder();

            // dynamic methods don't have a declaring type
            if (method.DeclaringType != null)
            {
                sb.Append(method.DeclaringType.GetPrettyName());
                sb.Append(".");
            }

            sb.Append(method.Name);

            if (method.IsGenericMethod)
            {
                sb.Append("<");
                sb.Append(string.Join(",", method.GetGenericArguments().Select(t => t.GetPrettyName()).ToArray()));
                sb.Append(">");
            }

            sb.Append("(");

            string[] parameters = method.GetParameters()
                .Select(p => (p.IsOut ? "out " : string.Empty) + p.ParameterType.GetPrettyName() + " " + p.Name)
                .ToArray();

            sb.Append(String.Join(", ", parameters));
            sb.Append(")");
            if (method is MethodInfo)
            {
                sb.Append(" : " + ((MethodInfo)method).ReturnType.GetPrettyName());
            }

            return sb.ToString();
        }

        private static string getDisplayName(Type type, Type[] genericArguments)
        {
            if (type == null) throw new ArgumentNullException("type");

            var sb = new StringBuilder();

            int declaringGenArgsCount = 0;
            if (type.DeclaringType != null && !type.IsGenericParameter)
            {
                if (type.DeclaringType.IsGenericType)
                {
                    declaringGenArgsCount = type.DeclaringType.GetGenericArguments().Length;
                    Type[] leadingGenericArguments = type.GetGenericArguments()
                        .Take(declaringGenArgsCount)
                        .ToArray();
                    sb.Append(getDisplayName(type.DeclaringType, leadingGenericArguments));
                }
                else
                {
                    sb.Append(type.DeclaringType.GetPrettyName());
                }

                sb.Append("+");
            }

            Type[] trailingGenericArguments = genericArguments
                                              ?? type.GetGenericArguments()
                                                     .Skip(declaringGenArgsCount)
                                                     .ToArray();

            if (trailingGenericArguments.Length == 0)
            {
                sb.Append(type.Name);
                return sb.ToString();
            }

            IEnumerable<string> displayNames = trailingGenericArguments.Select(t => t.GetPrettyName());

            sb.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(",", displayNames.ToArray()));
            sb.Append(">");

            return sb.ToString();
        }

        public static string GetPrettyName(this StackFrame frame)
        {
            if (frame == null) throw new ArgumentNullException("frame");

            var sb = new StringBuilder();

            MethodBase method = frame.GetMethod();
            sb.Append(method.GetPrettyName());

            string fileName = frame.GetFileName();
            if (fileName != null)
            {
                sb.Append(" in ");
                sb.Append(fileName);
                sb.Append(":");
                sb.Append(frame.GetFileLineNumber());
                sb.Append(":");
                sb.Append(frame.GetFileColumnNumber());
            }

            return sb.ToString();
        }
    }
}
