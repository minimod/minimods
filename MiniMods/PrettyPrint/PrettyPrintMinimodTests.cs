using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Minimod.PrettyText;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.PrettyPrint
{
    [TestFixture]
    public class PrettyPrintMinimodSamples
    {
        [Test]
        public void Sample1_UglyConsoleWriteLines()
        {
            var value = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            Console.WriteLine(value);
            Console.WriteLine(value.PrettyPrint());
        }

        [Test]
        public void Sample2_UglyArray()
        {
            var value = new[] { TimeSpan.MinValue, DateTime.Now.TimeOfDay };
            Console.WriteLine(value);
            Console.WriteLine(value.PrettyPrint());
        }
    }

    [TestFixture]
    public class PrettyPrintMinimodTests
    {
        [Test]
        public void PrettyPrint_String_HandlesNullAndEmpty()
        {
            testAndLog(() => "a", "a");
            testAndLog(() => (string)null, "<null, string>");
            testAndLog(() => "", "<String.Empty>");
        }

        [Test]
        public void PrettyPrint_NullObject_ShouldPrintNullOnly()
        {
            testAndLog(() => ((object)null), "<null>");
        }

        [Test]
        public void PrettyPrint_Guids_ShouldPrintNiceOutput()
        {
            testAndLog(() => Guid.Empty, "<Guid.Empty>");
            testAndLog(() => new Guid("79F8F96D-1BC7-458E-B1D6-65F867AF0FC1"), "79F8F96D-1BC7-458E-B1D6-65F867AF0FC1");
        }

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

        [Test]
        public void PrettyPrint_SmallCollection_ShouldPrintInSingleLine()
        {
            testAndLog(() => new[] { "a", "b", "c" }, "[a, b, c]", s => s);
        }

        [Test]
        public void PrettyPrint_SmallCollectionButItemsWithLineBreak_ShouldPrintMultiline()
        {
            testAndLog(() => new[] { "a" + Environment.NewLine + "b", "cd" }, @"[
  a
  b,
  cd
]");
        }

        [Test]
        public void PrettyPrint_LongerCollection_ShouldPrintMultiLine()
        {
            testAndLog(() => new[] { new string('a', 31), new string('b', 31), new string('c', 31) }, @"[
  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa,
  bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb,
  ccccccccccccccccccccccccccccccc
]");
        }

        [Test]
        public void PrettyPrint_LongerTypeNameCollection_ShouldPrintMultiLine()
        {
            testAndLog(() => new[] { 
                                       typeof(IDictionary<string, IList<object>>), 
                                       typeof(IDictionary<string, IList<object>>) 
                                   }, @"[
  IDictionary<string,IList<object>>,
  IDictionary<string,IList<object>>
]");
        }

        [Test]
        public void PrettyPrint_DictionaryFewShortValues_SingleLine()
        {
            testAndLog(() => new Dictionary<string, string[]>
                                 {
                                     {"key1", new[] {"a", "b"}}, 
                                     {"key2", new[] {"c", "d"}}, 
                                 }, @"[key1 => [a, b], key2 => [c, d]]");
        }

        [Test]
        public void PrettyPrint_NoContents_PrintsTypeNameOnly()
        {
            testAndLog(() => new NoContents(), @"<NoContents>");
        }

        [Test]
        public void PrettyPrint_EmptyAnonymousType_PrintsNicely()
        {
            testAndLog(() => new { }, @"<Anonymous>");
        }

        [Test]
        public void PrettyPrint_FilledAnonymousType_PrintsNicely()
        {
            testAndLog(() => new { myint = 1 }, @"{ myint = 1 }");
        }

        [Test]
        public void PrettyPrint_FilledAnonymousTypeMultiline_PrintsNicely()
        {
            testAndLog(() => new { myint = 1 }, @"{
  myint = 1
}", s => s.PreferMultiline(true));
        }

        [Test]
        public void PrettyPrint_PersonWithName_PropertiesMultiLine()
        {
            testAndLog(() => new PersonWithNameField { Name = "Lars" }, @"Lars <PersonWithNameField>");
        }

        [Test]
        public void PrettyPrint_PersonWithNameAndAddress_PropertiesMultiLine()
        {
            testAndLog(() => new PersonWithNameAndAdress
                                 {
                                     Name = "Lars",
                                     Address = @"Lehmstr. 1d
45731 Waltrop
Germany"
                                 }, @"Lars <PersonWithNameAndAdress> {
  Address = 
    Lehmstr. 1d
    45731 Waltrop
    Germany
}");
        }

        [Test]
        public void PrettyPrint_PersonWithAdressList_PropertiesMultiLine()
        {
            testAndLog(() => new PersonWithNameAndAdressList
            {
                Name = "Lars",
                Address = new[] { "Lehmstr. 1d", "45731 Waltrop", "Germany" }
            }, @"Lars <PersonWithNameAndAdressList> {
  Address = [
    Lehmstr. 1d,
    45731 Waltrop,
    Germany
  ]
}", s => s.PreferMultiline(true));
        }

        [Test]
        public void PrettyPrint_SingleIgnoredProp_ShouldPrintTypeName()
        {
            testAndLog(() => new PersonWithNameField { Name = "Name" },
                "<PersonWithNameField>",
                s => s.IgnoreMember((PersonWithNameField p) => p.Name));
        }

        [Test]
        public void PrettyPrint_CustomPropFormatter_ShouldUseRegisteredFormatter()
        {
            testAndLog(() => new PersonWithNameField { Name = "Name" },
                "_Name_ <PersonWithNameField>",
                s => s.RegisterPropertyFormatterFor((PersonWithNameField p) => p.Name, _ => "_" + _ + "_"));
        }

        [Test]
        public void PrettyPrint_Directory_ShouldPrintSelectedPropsOnly()
        {
            var sysDir = new DirectoryInfo(Environment.SystemDirectory);
            var prettyPrint = sysDir.PrettyPrint();
            Console.WriteLine(prettyPrint);
            prettyPrint.Should()
                .StartWith(sysDir.FullName + @" <DirectoryInfo>")
                .And.Contain("Exists = True")
                .And.Not.Contain("Directory = ")
                .And.Not.Contain("Root = ")
                .And.Not.Contain("CreationTimeUtc = ")
                ;
        }

        [Test]
        public void PrettyPrint_File_ShouldPrintSelectedPropsOnly()
        {
            var file = new DirectoryInfo(Environment.SystemDirectory).GetFiles()[0];
            var prettyPrint = file.PrettyPrint();
            Console.WriteLine(prettyPrint);
            prettyPrint.Should()
                .StartWith(file.Name + @" <FileInfo>")
                .And.Contain("Exists = True")
                .And.Contain("Directory = " + file.DirectoryName)
                .And.Not.Contain("DirectoryName = ")
                .And.Not.Contain("IsReadOnly = ")
                .And.Not.Contain("Attributes = ")
                ;
        }

        private void testAndLog<T>(Expression<Func<T>> func, string expected)
        {
            testAndLog(func, expected, s => s);
        }

        private void testAndLog<T>(Expression<Func<T>> func, string expected, Expression<Func<PrettyPrintMinimod.Settings, PrettyPrintMinimod.Settings>> customize)
        {
            Console.Write(func.Body + ".PrettyPrint(");
            string settings = customize.ToString();
            if (settings != "s => s")
            {
                Console.Write(settings);
            }
            Console.WriteLine(")");
            string actual = func.Compile().Invoke().PrettyPrint(customize.Compile());

            Console.WriteLine(actual.SplitLines().Select(line => "// " + line).JoinLines());
            Console.WriteLine();

            actual.Should().Be(expected);
        }
    }

    public class NoContents
    {

    }

    public class PersonWithNameField
    {
        public string Name;
    }

    public class PersonWithNameAndAdress
    {
        public string Name;
        public string Address;
    }

    public class PersonWithNameAndAdressList
    {
        public string Name;
        public string[] Address;
    }
}
