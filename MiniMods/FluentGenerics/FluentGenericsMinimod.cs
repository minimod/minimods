using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Minimod.PrettyTypeSignatures;

namespace Minimod.FluentGenerics
{
    /// <summary>
    /// <h1>Minimod.FluentGenerics, Version 1.0.1, Copyright © Lars Corneliussen 2011</h1>
    /// <para>A minimod for fluently interacting with genric types.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class FluentGenericsMinimod
    {
        public static bool IsOfGenericType(this Type type, Type genericTypeDefinition)
        {
            return findGenericTypesForDefinition(type, genericTypeDefinition).Length > 0;
        }

        public static Type[] GetGenericTypesFor(this Type type, Type genericTypeDefinition)
        {
            Type[] foundTypes = findGenericTypesForDefinition(type, genericTypeDefinition);

            if (foundTypes.Length == 0)
            {
                throw new ArgumentException(
                    string.Format(
                                     "The type '{0}' is not a generic type of {1}.",
                                     type.GetPrettyName(),
                                     genericTypeDefinition.GetPrettyName()),
                    "type");
            }

            return foundTypes;
        }

        public static Type GetGenericTypeFor(this Type type, Type genericTypeDefinition)
        {
            Type[] foundTypes = GetGenericTypesFor(type, genericTypeDefinition);

            if (foundTypes.Length > 1)
            {
                throw new ArgumentException(
                    string.Format(
                                     "The type '{0}' has multiple implementations of {1}:{2}",
                                     type.GetPrettyName(),
                                     genericTypeDefinition.GetPrettyName(),
                                     string.Join(", ", foundTypes.Select(t => t.GetPrettyName()).ToArray())
                                     ),
                    "type");
            }

            return foundTypes[0];
        }

        private static Type[] findGenericTypesForDefinition(Type type, Type genericTypeDefinition)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (genericTypeDefinition == null) throw new ArgumentNullException("genericTypeDefinition");

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    string.Format(
                                     "The type '{0}' is not a generic type definition.", genericTypeDefinition.FullName),
                    "genericTypeDefinition");
            }

            Type matchingBaseType = getTypeHierarchy(type)
                .Reverse()
                .Where(t => t.IsGenericType && genericTypeDefinition == t.GetGenericTypeDefinition())
                .FirstOrDefault();

            if (matchingBaseType != null)
            {
                return new[] { matchingBaseType };
            }

            TypeFilter filter = (i, o) => i.IsGenericType && genericTypeDefinition == i.GetGenericTypeDefinition();
            return type
                .FindInterfaces(filter, null)
                .ToArray();
        }

        public static Type[] GetGenericArgumentsFor(this Type type, Type genericTypeDefinition)
        {
            return GetGenericTypeFor(type, genericTypeDefinition).GetGenericArguments();
        }

        /// <summary>
        /// Beschafft die Typ-Hierarchie von object bis <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Der konkrete Typ, für den die Hierarchie zu beschaffen ist.
        /// </param>
        /// <returns>
        /// Eine Liste von Typen von object bis <paramref name="type"/>.
        /// </returns>
        private static IEnumerable<Type> getTypeHierarchy(this Type type)
        {
            var types = new List<Type>();
            for (Type current = type; current != null; current = current.BaseType)
            {
                types.Add(current);
            }

            // drehe sie dann um
            types.Reverse();

            return types;
        }
    }
}
