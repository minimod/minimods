using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.PrettyText
{
    [TestFixture]
    public class PrettyTextMinimodTests
    {
        [Test]
        public void ShortenTo_Empty_StaysEmpty()
        {
            "".ShortenTo(0).Should().Be("");
            "".ShortenTo(1).Should().Be("");
        }

        [Test]
        public void ShortenTo_Matching_DoesNotChange()
        {
            "a".ShortenTo(1).Should().Be("a");
            "ab".ShortenTo(2).Should().Be("ab");
            "abc".ShortenTo(3).Should().Be("abc");
        }

        [Test]
        public void WrapAt_SecondWordExceedsMargin_EachWordOnALine()
        {

            "cd efgd".WrapAt(4).Should().Have.SameSequenceAs(new string[2]
                                                                     {
                                                                         "cd",
                                                                         "efgd"
                                                                     });
        }

        [Test]
        public void WrapAt_TwoWordsAtMarginLength_WrapsAndRemovesSpace()
        {
            "ab cd".WrapAt(2).Should().Have.SameSequenceAs(new[]{
                                                                        "ab",
                                                                        "cd"
                                                                    });
        }

        [Test]
        public void WrapAt_OneWordLongerThanMargin_WrapsWordAtMargin()
        {
            "abc".WrapAt(2).Should().Have.SameSequenceAs(new string[2]
                                                             {
                                                                 "ab",
                                                                 "c"
                                                             });
        }

        [Test]
        public void WrapAt_1_ALinePerChar()
        {
            "abc".WrapAt(1).Should().Have.SameSequenceAs(new string[3]
                                                             {
                                                                 "a",
                                                                 "b",
                                                                 "c"
                                                             });
        }
    }
}
