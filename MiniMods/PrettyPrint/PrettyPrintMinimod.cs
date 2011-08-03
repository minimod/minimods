using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Minimod.PrettyText;
using Minimod.PrettyTypeSignatures;

namespace Minimod.PrettyPrint
{
    /// <summary>
    /// <h1>Minimod.PrettyPrint, Version 0.8.1, Copyright © Lars Corneliussen 2011</h1>
    /// <para>Creates nice textual representations of any objects. Mostly meant for debug/informational output.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public static class PrettyPrintMinimod
    {
        public static Settings DefaultSettings;

        static PrettyPrintMinimod()
        {
            var settings = new Settings();
            settings.RegisterFormatterFor<Type>(t => t.GetPrettyName());
            DefaultSettings = settings;
        }

        public static Settings CreateCustomSettings()
        {
            return new Settings(DefaultSettings);
        }

        public class Settings
        {
            private readonly Settings _inner;
            private Dictionary<Type, Func<object, string>> _customFormatters = new Dictionary<Type, Func<object, string>>();

            public bool? PreferesMultiline { get; private set; }

            public Settings()
            {

            }

            public Settings(Settings inner)
                : this()
            {
                _inner = inner;
            }

            public Settings RegisterToStringFor(Type type)
            {
                _customFormatters.Add(type, o => o.ToString());
                return this;
            }

            public Settings RegisterFormatterFor(Type type, Func<object, string> formatter)
            {
                _customFormatters.Add(type, formatter);
                return this;
            }

            public Settings RegisterFormatterFor<T>(Func<T, string> formatter)
            {
                _customFormatters.Add(typeof(T), o => formatter((T)o));
                return this;
            }

            public Func<object, string> GetCustomFormatter(object anyObject)
            {
                return _customFormatters
                  .Where(t => t.Key.IsAssignableFrom(anyObject.GetType()))
                  .Select(kv => kv.Value)
                  .FirstOrDefault()
                  ?? (_inner != null ? _inner.GetCustomFormatter(anyObject) : null);
            }

            public Settings PreferMultiline(bool multiline)
            {
                PreferesMultiline = multiline;
                return this;
            }
        }

        public static string PrettyPrint<T>(this T anyObject)
        {
            return anyObject.PrettyPrint(typeof(T), DefaultSettings);
        }

        public static string PrettyPrint<T>(this T anyObject, Settings settings)
        {
            return anyObject.PrettyPrint(typeof(T), settings);
        }

        public static string PrettyPrint<T>(this T anyObject, Func<Settings, Settings> customize)
        {
            return anyObject.PrettyPrint(typeof(T), customize(DefaultSettings));
        }

        public static string PrettyPrint(this object anyObject, Type declaredType)
        {
            return anyObject.PrettyPrint(declaredType, DefaultSettings);
        }

        public static string PrettyPrint(this object anyObject, Type declaredType, Func<Settings, Settings> customize)
        {
            return anyObject.PrettyPrint(declaredType, customize(DefaultSettings));
        }

        public static string PrettyPrint(this object anyObject, Type declaredType, Settings settings)
        {
            if (anyObject == null)
            {
                return "<null" + (declaredType != typeof(object) ? ", " + declaredType.GetPrettyName() : "") + ">";
            }

            var formatter = settings.GetCustomFormatter(anyObject);
            if (formatter != null)
            {
                return formatter(anyObject);
            }

            var actualType = anyObject.GetType();

            if ((anyObject is string) || actualType.IsPrimitive)
            {
                return anyObject.ToString();
            }

            if (anyObject is IEnumerable)
            {
                return prettyPrint(anyObject as IEnumerable, declaredType, settings);
            }

            var properties =
               from prop in actualType.GetMembers().OfType<PropertyInfo>()
               where !prop.GetGetMethod().IsStatic && prop.GetIndexParameters().Length == 0
               select new { name = prop.Name, type = prop.PropertyType, value = prop.GetValue(anyObject, new object[0]) };

            var fields =
               from prop in actualType.GetMembers().OfType<FieldInfo>()
               where !prop.IsStatic
               select new { name = prop.Name, type = prop.FieldType, value = prop.GetValue(anyObject) };

            var members = fields.Union(properties).ToArray();


            var nameProperty = members.Where(_ => _.name == "Name").FirstOrDefault();
            var keyProperty = members.Where(_ => _.name == "Key").FirstOrDefault();
            var valueProperty = members.Where(_ => _.name == "Value").FirstOrDefault();

            if (keyProperty != null && valueProperty != null && members.Length == 2)
            {
                return keyProperty.value.PrettyPrint(keyProperty.type, settings) + " => " + valueProperty.value.PrettyPrint(valueProperty.type, settings);
            }

            StringBuilder sb = new StringBuilder();
            if (nameProperty != null && nameProperty.value != null)
            {
                sb.Append(nameProperty.value.PrettyPrint(nameProperty.type, settings) + " <" + actualType.GetPrettyName() + ">");
            }
            else
            {
                sb.Append(actualType.GetPrettyName());
            }

            var moreProps = members.Where(_ => _.name != "Name").ToArray();
            if (moreProps.Length != 0)
            {
                sb.AppendLine(" {");
                foreach (var prop in moreProps)
                {
                    string value = prop.value.PrettyPrint(prop.type, settings);

                    if (prop.type == typeof(string)
                        && value.Contains(Environment.NewLine))
                    {
                        value = Environment.NewLine + value.IndentLinesBy(2);
                    }

                    string propString =
                        (prop.name + " = " + value).IndentLinesBy(2);

                    sb.AppendLine(propString);
                }
                sb.Append("}");
            }

            return sb.ToString();
        }

        private static string prettyPrint(IEnumerable objects, Type declaredType, Settings settings)
        {
            string[] items = objects.Cast<object>().Select(_ => _.PrettyPrint(settings)).ToArray();

            if (settings.PreferesMultiline
                ?? (
                    (items.Length > 1 && items.Any(i => i.Length > 30))
                    || (items.Length > 10)
                    || items.Any(i => i.Contains(Environment.NewLine)))
                    )
            {
                return "["
                       + Environment.NewLine
                       + String.Join("," + Environment.NewLine, items.Select(i => i.IndentLinesBy(2)).ToArray())
                       + Environment.NewLine
                       + "]";
            }

            return "[" + String.Join(", ", items) + "]";
        }
    }
}