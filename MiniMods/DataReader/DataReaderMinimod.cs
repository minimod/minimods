using System;
using System.Data;

namespace Minimod.DataReader
{
    /// <summary>
    /// <h1>Minimod.DataReader, Version 0.0.1, Copyright © Uwe Zimmermann, 2012</h1>
    /// <para>A minimod with some extension methods for <see cref="IDataReader"/>.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class DataReaderMinimod
    {
        /// <summary>
        /// Gets the string value of the specified field. If the value is <see cref="DBNull.Value"/> it returns <c>null</c>.
        /// </summary>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A string containing the value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>
        public static string GetStringOrNull(this IDataReader self, string name)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            var ordinal = self.GetOrdinal(name);

            return self.IsDBNull(ordinal) ? null : self.GetString(ordinal);
        }

        /// <summary>
        /// Gets the string value of the specified field. If the value is <see cref="DBNull.Value"/> it returns <c>null</c>.
        /// </summary>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="fieldIndex">The index of the field.</param>
        /// <returns>A string containing the value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>        
        public static string GetStringOrNull(this IDataReader self, int fieldIndex)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return self.IsDBNull(fieldIndex) ? null : self.GetString(fieldIndex);
        }

        /// <summary>
        /// Gets the value of the specified field as <c>Nullable</c> of <typeparamref name="TResult"/>. If the value is <see cref="DBNull.Value"/> 
        /// it returns <c>null</c>.
        /// </summary>
        /// <typeparam name="TResult">The type to return. It have to be a struct.</typeparam>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>
        public static TResult? GetNullableValue<TResult>(this IDataReader self, string fieldName) where TResult : struct
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            var ordinal = self.GetOrdinal(fieldName);

            return self.IsDBNull(ordinal) ? null : (TResult?)self.GetValue(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified field as <c>Nullable</c> of <typeparamref name="TResult"/>. If the value is <see cref="DBNull.Value"/> 
        /// it returns <c>null</c>.
        /// </summary>
        /// <typeparam name="TResult">The type to return. It have to be a struct.</typeparam>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="fieldIndex">The index of the field.</param>
        /// <returns>The value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>
        public static TResult? GetNullableValue<TResult>(this IDataReader self, int fieldIndex) where TResult : struct
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return self.IsDBNull(fieldIndex) ? null : (TResult?)self.GetValue(fieldIndex);
        }

        /// <summary>
        /// Gets the value of the specified field as <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type to return. It have to be a struct.</typeparam>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>
        public static TResult GetValue<TResult>(this IDataReader self, string fieldName) where TResult : struct
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            var ordinal = self.GetOrdinal(fieldName);

            return (TResult)self.GetValue(ordinal);
        }

        /// <summary>
        /// Gets the value of the specified field as <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type to return. It have to be a struct.</typeparam>
        /// <param name="self">An <see cref="IDataReader"/>.</param>
        /// <param name="fieldIndex">The index of the field.</param>
        /// <returns>The value of the specified field or <c>null</c> if the value is <see cref="DBNull.Value"/>.</returns>
        /// <exception cref="ArgumentNullException">When parameter <paramref name="self"/> is <c>null</c>.</exception>
        public static TResult GetValue<TResult>(this IDataReader self, int fieldIndex) where TResult : struct
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return (TResult)self.GetValue(fieldIndex);
        }
    }
}