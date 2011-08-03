using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minimod.PrettyText;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.PrettyPrint
{
    [TestFixture]
    public class PrettyPrintMinimodTests
    {
        [Test]
        public void PrettyPrint_String_DoesNotChange()
        {
            "".PrettyPrint().Should().Be("");
            "a".PrettyPrint().Should().Be("a");
        }

        [Test]
        public void PrettyPrint_NullObject_ShouldPrintNullOnly()
        {
            ((object)null).PrettyPrint().Should().Be("<null>");
        }

        [Test]
        public void PrettyPrint_NullString_ShouldPrintNullAndType()
        {
            ((string)null).PrettyPrint().Should().Be("<null, String>");
        }

        [Test]
        public void PrettyPrint_SmallCollection_ShouldPrintInSingleLine()
        {
            new[] { "a", "b", "c" }.PrettyPrint().Should().Be("[a, b, c]");
        }

        [Test]
        public void PrettyPrint_SmallCollectionButItemsWithLineBreak_ShouldPrintMultiline()
        {
            new[] { "a" + Environment.NewLine + "b", "cd" }.PrettyPrint().Should().Be(@"[
  a
  b,
  cd
]");
        }

        [Test]
        public void PrettyPrint_LongerCollection_ShouldPrintMultiLine()
        {
            new[] { new string('a', 31), new string('b', 31), new string('c', 31) }.PrettyPrint().Should().Be(@"[
  aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa,
  bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb,
  ccccccccccccccccccccccccccccccc
]");
        }

        [Test]
        public void PrettyPrint_LongerTypeNameCollection_ShouldPrintMultiLine()
        {
            string actual = new[] { 
                    typeof(IDictionary<string, IList<object>>), 
                    typeof(IDictionary<string, IList<object>>) 
                }
                .PrettyPrint();

            string expected = @"[
  IDictionary<String,IList<Object>>,
  IDictionary<String,IList<Object>>
]";

            actual.Should().Be(expected);
        }

        [Test]
        public void PrettyPrint_DictionaryFewShortValues_SingleLine()
        {
            string actual = new Dictionary<string, string[]>
                                {
                                    {"key1", new[] {"a", "b"}}, 
                                    {"key2", new[] {"c", "d"}}, 
                                }
                .PrettyPrint();

            string expected = @"[key1 => [a, b], key2 => [c, d]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void PrettyPrint_NoContents_PrintsTypeNameOnly()
        {
            string actual = new NoContents()
                .PrettyPrint();

            string expected = @"NoContents";

            actual.Should().Be(expected);
        }

        [Test]
        public void PrettyPrint_PersonWithName_PropertiesMultiLine()
        {
            string actual = new PersonWithNameField { Name = "Lars" }
                .PrettyPrint();

            string expected = @"Lars <PersonWithNameField>";

            actual.Should().Be(expected);
        }

        [Test]
        public void PrettyPrint_PersonWithNameAndAddress_PropertiesMultiLine()
        {
            string actual = new PersonWithNameAndAdress
                                {
                                    Name = "Lars",
                                    Address = @"Lehmstr. 1d
45731 Waltrop
Germany"
                                }
                .PrettyPrint();

            Console.WriteLine(actual);

            string expected = @"Lars <PersonWithNameAndAdress> {
  Address = 
    Lehmstr. 1d
    45731 Waltrop
    Germany
}";

            actual.Should().Be(expected);
        }

        [Test]
        public void PrettyPrint_PersonWithAdressList_PropertiesMultiLine()
        {
            string actual = new PersonWithNameAndAdressList
            {
                Name = "Lars",
                Address = new[] { "Lehmstr. 1d", "45731 Waltrop", "Germany" }
            }
                .PrettyPrint(PrettyPrintMinimod.CreateCustomSettings().PreferMultiline(true));

            Console.WriteLine(actual);

            string expected = @"Lars <PersonWithNameAndAdressList> {
  Address = [
    Lehmstr. 1d,
    45731 Waltrop,
    Germany
  ]
}";

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
