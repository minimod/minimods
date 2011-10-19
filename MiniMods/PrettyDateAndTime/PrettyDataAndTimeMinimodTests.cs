using System;
using System.Linq;
using System.Linq.Expressions;
using Minimod.PrettyText;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.PrettyDateAndTime
{
    [TestFixture]
    public class PrettyDataAndTimeMinimodTests
    {
        [Test]
        public void PrettyPrint_DateTime_ShouldPrintNiceOutput()
        {
            testAndLog(() => DateTime.MinValue, "<DateTime.MinValue>");
            testAndLog(() => DateTime.MaxValue, "<DateTime.MaxValue>");
            testAndLog(() => new DateTime(2000, 1, 1), "2000-01-01");
            testAndLog(() => new DateTime(2000, 1, 1, 14, 0, 0), "2000-01-01 14:00");
            testAndLog(() => new DateTime(2000, 1, 1, 14, 0, 15), "2000-01-01 14:00:15");
            testAndLog(() => new DateTime(2000, 1, 1, 14, 0, 15, 20), "2000-01-01 14:00:15.020");

            Console.WriteLine("// We are in Germany, currently +01:00");
            Console.WriteLine("// TimeZone-handling");
            testAndLog(() => new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), "2000-01-01 (UTC)");
            testAndLog(() => new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local), "2000-01-01 (+01:00)");

            Console.WriteLine("// DateTimeOffset-handling");
            testAndLog(() => new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero), "<DateTimeOffset> { 2000-01-01 01:00 (+01:00), 2000-01-01 (UTC) }");
            testAndLog(() => new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(-1)), "<DateTimeOffset> { 2000-01-01 02:00 (+01:00), 2000-01-01 01:00 (UTC) }");
        }

        [Test]
        public void PrettyPrint_TimeSpan_ShouldPrintNiceOutput()
        {
            testAndLog(() => TimeSpan.Zero, "<TimeSpan.Zero>");
            testAndLog(() => TimeSpan.MinValue, "<TimeSpan.MinValue>");
            testAndLog(() => TimeSpan.MaxValue, "<TimeSpan.MaxValue>");

            testAndLog(() => TimeSpan.FromMilliseconds(1), "1 ms");
            testAndLog(() => TimeSpan.FromMilliseconds(1000), "1 s");
            testAndLog(() => TimeSpan.FromMilliseconds(1020), "1.020 s");
            testAndLog(() => TimeSpan.FromSeconds(60), "1 min");
            testAndLog(() => TimeSpan.FromSeconds(80), "1:20 min");
            testAndLog(() => TimeSpan.FromSeconds(80).Add(TimeSpan.FromMilliseconds(1)), "1:20.001 min");


            testAndLog(() => TimeSpan.FromMinutes(5), "5 min");
            testAndLog(() => TimeSpan.FromMinutes(80), "1:20 h");
            testAndLog(() => new TimeSpan(0, 1, 0, 1, 0), "1:00:01 h");
            testAndLog(() => new TimeSpan(0, 1, 0, 0, 1), "1:00:00.001 h");

            testAndLog(() => TimeSpan.FromMinutes(120), "2 h");
            testAndLog(() => TimeSpan.FromHours(10), "10 h");
            testAndLog(() => TimeSpan.FromHours(24), "1 d");
            testAndLog(() => TimeSpan.FromHours(25), "1.01 d");
            testAndLog(() => TimeSpan.FromHours(48), "2 d");

            testAndLog(() => new TimeSpan(2, 0, 1, 0, 0), "2.00:01 d");
            testAndLog(() => new TimeSpan(2, 0, 0, 1, 0), "2.00:00:01 d");
            testAndLog(() => new TimeSpan(2, 0, 0, 0, 1), "2.00:00:00.001 d");
        }

        private void testAndLog<T>(Expression<Func<T>> func, string expected)
        {
            Console.Write(func.Body);
            object actualObj = func.Compile().Invoke();
            string actual = "";
            if (actualObj is DateTime)
                actual = ((DateTime)actualObj).GetPrettyString();
            else if (actualObj is DateTimeOffset)
                actual = ((DateTimeOffset)actualObj).GetPrettyString();
            else if (actualObj is TimeSpan)
                actual = ((TimeSpan)actualObj).GetPrettyString();
            else
            {
                throw new NotSupportedException("No 'GetPrettyString' for " + actualObj.GetType());
            }

            Console.WriteLine(actual.SplitLines().Select(line => "// " + line).JoinLines());
            Console.WriteLine();

            actual.Should().Be(expected);
        }
    }
}