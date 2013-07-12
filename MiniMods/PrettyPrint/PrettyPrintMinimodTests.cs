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
                s => s.RegisterMemberFormatterFor((PersonWithNameField p) => p.Name, _ => "_" + _ + "_"));
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
        public void PrettyPrint_NotExistingDirectory_ShouldPrintNameAndNotExists()
        {
            var sysDir = new DirectoryInfo(Path.Combine(Environment.SystemDirectory, "does-not-exist"));
            var prettyPrint = sysDir.PrettyPrint();
            Console.WriteLine(prettyPrint);
            prettyPrint.Should()
                .StartWith(sysDir.FullName + @" <DirectoryInfo>")
                .And.Contain("Exists = False")
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
                .And.Contain("CreationTime = ")
                .And.Contain("LastAccessTime = ")
                .And.Contain("LastWriteTime = ")
                .And.Not.Contain("CreationTimeUtc = ")
                .And.Not.Contain("LastAccessTimeUtc = ")
                .And.Not.Contain("LastWriteTimeUtc = ")
                .And.Not.Contain("DirectoryName = ")
                .And.Not.Contain("IsReadOnly = ")
                .And.Not.Contain("Attributes = ")
                ;
        }

        [Test]
        public void PrettyPrint_NotExistingFile_ShouldPrintNameAndNotExists()
        {
            var file = new FileInfo(Path.Combine(Environment.SystemDirectory, "not-existing-file"));
            var prettyPrint = file.PrettyPrint();
            Console.WriteLine(prettyPrint);
            prettyPrint.Should()
                .StartWith(file.Name + @" <FileInfo>")
                .And.Contain("Exists = False")
                .And.Contain("Directory = " + file.DirectoryName)
                .And.Not.Contain("Length = ")
                .And.Not.Contain("CreationTime = ")
                .And.Not.Contain("LastAccessTime  = ")
                .And.Not.Contain("LastWriteTime  = ")
                .And.Not.Contain("DirectoryName = ")
                .And.Not.Contain("IsReadOnly = ")
                .And.Not.Contain("Attributes = ")
                ;
        }

        [Test]
        public void PrettyPrint_Enum()
        {
            testAndLog(() => TestEnum.First, "<TestEnum.First = 0>");
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

    public enum TestEnum
    {
        First = 0,
        Second = 1
    }

    public class HasEnums
    {
        public TestEnum EnumProperty { get; set; }
        public TestEnum EnumField;
    }
}
