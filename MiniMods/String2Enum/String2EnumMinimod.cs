using System;

namespace Minimod.String2Enum
{
    /// <summary>
    /// <h1>Minimod.String2Enum, Version 0.9.2, Copyright © Michel Bretschneider 2012</h1>
    /// <para>A minimod for parsing Strings as Enums.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class String2EnumMinimod
    {
        /**
        <summary>
        usage:
        <code>  
         var userInput = "Monday";
         var selectedWeekday = userInput.ToEnum<Weekday>();
         Assert.AreEqual(Weekday.Monday, selectedWeekday);  
        </code>
        </summary>
        <typeparam name="T">Type beeing an Enum</typeparam>
        <param name="value">String representing the Enum value</param>
        <returns>related Enum value</returns>
        */
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, IConvertible, IFormattable, IComparable
        {
            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value", "Cannot parse null or empty string.");
            }

            Type enumType = typeof (TEnum);
            if(!enumType.IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum.");
            }

            if(Enum.IsDefined(enumType, value))
            {
                return (TEnum) Enum.Parse(enumType, value);
            }
            var message = string.Format("Enum {0} has no value {1}", enumType, value);
            throw new ArgumentException(message);
        }
    }
}
