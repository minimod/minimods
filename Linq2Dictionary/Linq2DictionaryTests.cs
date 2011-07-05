using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.Linq2Dictionary
{
    [TestFixture]
    public class Linq2DictionaryTests
    {
        [Test]
        public void Union_FirstIsNull_ThrowsArgumentException()
        {
            IDictionary<string, string> first = null;


            first.Executing(f => f.Union(new Dictionary<string, string>()))
                .Throws<ArgumentNullException>();
        }

        [Test]
        public void Union_SecondIsNull_ThrowsArgumentException()
        {
            IDictionary<string, string> first = new Dictionary<string, string>();


            first.Executing(f => f.Union(null))
                .Throws<ArgumentNullException>();
        }

        [Test]
        public void Union_BothEmpty_IsEmpty()
        {
            IDictionary<string, string> first = new Dictionary<string, string>();
            IDictionary<string, string> second = new Dictionary<string, string>();

            IDictionary<string, string> result = first.Union(second);
            result.Should().Be.Empty();
        }

        [Test]
        public void Union_SecondEmpty_ContentEqualsFirst()
        {
            IDictionary<string, string> first = new Dictionary<string, string>()
                                                    {
                                                        {"a", "1"}
                                                    };

            IDictionary<string, string> second = new Dictionary<string, string>();

            IDictionary<string, string> result = first.Union(second);
            result.Should().Have.SameValuesAs(first);
        }

        [Test]
        public void Union_FirstEmpty_ContentEqualsFirst()
        {
            IDictionary<string, string> first = new Dictionary<string, string>();

            IDictionary<string, string> second = new Dictionary<string, string>(){
                                                        {"a", "1"}
                                                    };

            IDictionary<string, string> result = first.Union(second);
            result.Should().Have.SameValuesAs(second);
        }

        [Test]
        public void Union_DistinctKeys_ContainsBoth()
        {
            IDictionary<string, string> first = new Dictionary<string, string>(){
                                                        {"a", "1"}
                                                    };

            IDictionary<string, string> second = new Dictionary<string, string>{
                                                        {"b", "2"}
                                                    };

            IDictionary<string, string> result = first.Union(second);
            result.Should().Have.SameValuesAs(new Dictionary<string, string>{
                                                        {"a", "1"},
                                                        {"b", "2"}
                                                    });
        }

        [Test]
        public void Union_KeyOverlap_Throws()
        {
            IDictionary<string, string> first = new Dictionary<string, string>(){
                                                        {"a", "1"}
                                                    };

            IDictionary<string, string> second = new Dictionary<string, string>{
                                                        {"a", "2"}
                                                    };

            first.Executing(_ => _.Union(second))
                .Throws<ArgumentException>();
        }
    }
}