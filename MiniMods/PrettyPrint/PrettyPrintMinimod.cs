using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Minimod.PrettyText;
using Minimod.PrettyTypeSignatures;

namespace Minimod.PrettyPrint
{
    /// <summary>
    /// <h1>Minimod.PrettyPrint, Version 0.8.6, Copyright © Lars Corneliussen 2011</h1>
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
            settings.RegisterFormatterFor<Guid>(formatter);
            settings.RegisterFormatterFor<DateTime>(formatter);
            settings.RegisterFormatterFor<DateTimeOffset>(formatter);
            settings.RegisterFormatterFor<TimeSpan>(formatter);
            DefaultSettings = settings;
        }

        private static string formatter(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue) return "<DateTime.MinValue>";
            if (dateTime == DateTime.MaxValue) return "<DateTime.MaxValue>";

            string kind = "";
            if (dateTime.Kind == DateTimeKind.Utc)
                kind = " (UTC)";
            else if (dateTime.Kind == DateTimeKind.Local)
                kind = dateTime.ToString(" (K)");


            if (dateTime.TimeOfDay == TimeSpan.Zero) return dateTime.ToString("yyyy-MM-dd") + kind;
            if (dateTime.Second + dateTime.Millisecond == 0) return dateTime.ToString("yyyy-MM-dd HH:mm") + kind;
            if (dateTime.Millisecond == 0) return dateTime.ToString("yyyy-MM-dd HH:mm:ss") + kind;

            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + kind;
        }

        private static string formatter(DateTimeOffset dateTime)
        {
            return "DateTimeOffset { " + formatter(dateTime.LocalDateTime) + ", " + formatter(dateTime.UtcDateTime) +
                   " }";
        }

        #region Do extract to PrettyTimeSpanMinimod
        private static string formatter(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.FromSeconds(1))
            {
                return milliseconds(timeSpan);
            }
            if (timeSpan < TimeSpan.FromMinutes(1))
            {
                return seconds(timeSpan);
            }
            if (timeSpan < TimeSpan.FromHours(1))
            {
                return minutes(timeSpan);
            }
            if (timeSpan < TimeSpan.FromHours(24))
            {
                return hours(timeSpan);
            }

            return days(timeSpan);
        }

        private static string milliseconds(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds % 1 == 0)
            {
                return (int)timeSpan.TotalMilliseconds + " ms";
            }

            return timeSpan.TotalMilliseconds.ToString() + " ms";
        }

        private static string seconds(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds % 1 == 0)
            {
                return (int)timeSpan.TotalSeconds + " s";
            }

            return string.Format("{0}.{1:D3} s", timeSpan.Seconds, timeSpan.Milliseconds);
        }

        private static string minutes(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes % 1 == 0)
            {
                return (int)timeSpan.TotalMinutes + " min";
            }

            return string.Format("{0}:{1:D2}" + (timeSpan.Milliseconds == 0 ? "" : ".{2:D3}") + " min", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        private static string hours(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours % 1 == 0)
            {
                return (int)timeSpan.TotalHours + " h";
            }

            return string.Format("{0}:{1:D2}" + ((timeSpan.TotalMinutes % 1 == 0)
                                                     ? ""
                                                     : ":{2:D2}" + (timeSpan.TotalSeconds % 1 == 0
                                                                      ? ""
                                                                      : ".{3:D3}")) + " h", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }


        private static string days(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays % 1 == 0)
            {
                return (int)timeSpan.TotalDays + " d";
            }

            return string.Format("{0}.{1:D2}" +
                                     ((timeSpan.TotalHours % 1 == 0)
                                          ? ""
                                          : ":{2:D2}" + ((timeSpan.TotalMinutes % 1 == 0)
                                                      ? ""
                                                      : ":{3:D2}" + (timeSpan.TotalSeconds % 1 == 0
                                                                       ? ""
                                                                       : ".{4:D3}"))) + " d", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        #endregion

        private static string formatter(Guid guid)
        {
            if (guid == Guid.Empty) return "<Guid.Empty>";
            return guid.ToString("D").ToUpperInvariant();
        }

        public static Settings CreateCustomSettings()
        {
            return new Settings(DefaultSettings);
        }

        public class Settings
        {
            private readonly Settings _inner;

            private Dictionary<Type, Func<object, string>> _customFormatters =
                new Dictionary<Type, Func<object, string>>();

            public bool? PreferesMultiline { get; private set; }

            public Settings()
            {
            }

            public Settings(Settings inner)
                : this()
            {
                _inner = inner;
            }

            public Settings RegisterToStringFor<T>()
            {
                return RegisterToStringFor(typeof(T));
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
            return anyObject.PrettyPrint(typeof(T), customize(CreateCustomSettings()));
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

            if ((anyObject is string))
            {
                var s = (string)anyObject;
                return s == String.Empty
                    ? "<String.Empty>"
                    : s;
            }

            if (actualType.IsPrimitive)
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

            var keyProperty = members.Where(_ => _.name == "Key").FirstOrDefault();
            var valueProperty = members.Where(_ => _.name == "Value").FirstOrDefault();

            if (keyProperty != null && valueProperty != null && members.Length == 2)
            {
                return keyProperty.value.PrettyPrint(keyProperty.type, settings) + " => " +
                       valueProperty.value.PrettyPrint(valueProperty.type, settings);
            }

            var contentProps = members.Where(_ => _.name != "Name").ToArray();

            var prependedProp = members.Where(_ => _.name == "Name").FirstOrDefault();

            List<string> parts = new List<string>();
            if (prependedProp != null && prependedProp.value != null)
            {
                parts.Add(prependedProp.value.PrettyPrint(prependedProp.type, settings));
            }

            string typeName = checkIfAnonymousType(actualType) && contentProps.Length > 0
                ? null
                : actualType.GetPrettyName();

            if (typeName != null)
            {
                parts.Add("<" + actualType.GetPrettyName() + ">");
            }

            if (contentProps.Length != 0)
            {
                List<string> printedProps = new List<string>();
                foreach (var prop in contentProps)
                {
                    string value = prop.value.PrettyPrint(prop.type, settings);

                    if (prop.type == typeof(string)
                        && value.Contains(Environment.NewLine))
                    {
                        value = Environment.NewLine + value.IndentLinesBy(2);
                    }

                    string propString =
                        (prop.name + " = " + value).IndentLinesBy(2);

                    printedProps.Add(propString);
                }

                if (printedProps.Any(s => s.Length > 30 || s.Contains(Environment.NewLine)))
                {
                    StringBuilder contentBuilder = new StringBuilder();
                    contentBuilder.AppendLine("{");
                    contentBuilder.AppendLine(printedProps.JoinLines());
                    contentBuilder.Append("}");
                    parts.Add(contentBuilder.ToString());
                }
                else
                {
                    parts.Add("{ " + String.Join(", ", printedProps.Select(s => s.Trim()).ToArray()) + " }");
                }
            }

            return String.Join(" ", parts.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray());
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


        private static bool checkIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}