using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace Minimod.ThreadVariable
{
    public delegate void MethodThatThrows();

    public static class AssertionExtensions
    {
        public static T Out<T>(this T @object)
        {
            return Out<T>(@object, "{0}");
        }

        public static T Out<T>(this T @object, string format)
        {
            Console.WriteLine(format, @object);
            return @object;
        }

        public static void ShouldBeFalse(this bool condition)
        {
            Assert.IsFalse(condition);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static T ShouldEqual<T>(this T actual, object expected)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        public static T ShouldNotEqual<T>(this T actual, object expected)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        public static void ShouldBeNull(this object anObject)
        {
            Assert.IsNull(anObject);
        }

        public static T ShouldNotBeNull<T>(this T anObject)
        {
            Assert.IsNotNull(anObject);
            return anObject;
        }

        public static T ShouldBeTheSameAs<T>(this T actual, object expected)
        {
            Assert.AreSame(expected, actual);
            return actual;
        }

        public static T ShouldNotBeTheSameAs<T>(this T actual, object expected)
        {
            Assert.AreNotSame(expected, actual);
            return actual;
        }

        public static T ShouldBeOfType<T>(this T actual, Type expected)
        {
            Assert.IsInstanceOfType(expected, actual);
            return actual;
        }

        public static void ShouldNotBeOfType(this object actual, Type expected)
        {
            Assert.IsNotInstanceOfType(expected, actual);
        }

        public static T ShouldContain<T>(this T actual, object expected) where T : IEnumerable
        {
            Assert.Contains(expected, actual.Cast<object>().ToList());
            return actual;
        }

        public static T ShouldBeGreaterThan<T>(this T arg1, T arg2) where T : IComparable
        {
            Assert.Greater(arg1, arg2);
            return arg1;
        }

        public static T ShouldBeLessThan<T>(this T arg1, T arg2) where T : IComparable
        {
            Assert.Less(arg1, arg2);
            return arg1;
        }

        public static void ShouldBeEmpty(this System.Collections.ICollection collection)
        {
            Assert.IsEmpty(collection);
        }

        public static void ShouldBeEmpty(this string aString)
        {
            Assert.IsEmpty(aString);
        }

        public static ICollection ShouldNotBeEmpty(this ICollection collection)
        {
            Assert.IsNotEmpty(collection);
            return collection;
        }

        public static string ShouldNotBeEmpty(this string aString)
        {
            Assert.IsNotEmpty(aString);
            return aString;
        }

        public static string ShouldContain(this string actual, string expected)
        {
            StringAssert.Contains(expected, actual);
            return actual;
        }

        public static string ShouldBeEqualIgnoringCase(this string actual, string expected)
        {
            StringAssert.AreEqualIgnoringCase(expected, actual);
            return actual;
        }

        public static string ShouldEndWith(this string actual, string expected)
        {
            StringAssert.EndsWith(expected, actual);
            return actual;
        }

        public static string ShouldStartWith(this string actual, string expected)
        {
            StringAssert.StartsWith(expected, actual);
            return actual;
        }

        public static Exception ShouldContainErrorMessage(this Exception exception, string expected)
        {
            StringAssert.Contains(expected, exception.Message);
            return exception;
        }

        public static Exception ShouldBeThrownBy(this Type exceptionType, MethodThatThrows method)
        {
            Exception exception = null;
            try
            {
                method();
            }
            catch (Exception e)
            {
                Assert.AreEqual(exceptionType, e.GetType(), e.ToString());
                exception = e;
            }
            if (exception == null)
            {
                Assert.Fail(String.Format("Expected {0} to be thrown.", exceptionType.FullName));
            }
            return exception;
        }

        public static void ShouldEqualSqlDate(this DateTime actual, DateTime expected)
        {
            TimeSpan timeSpan = actual - expected;
            Assert.Less(Math.Abs(timeSpan.TotalMilliseconds), 3);
        }
    }
}