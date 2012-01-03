using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.FluentNullOrEmpty
{
    [TestFixture]
    public class FluentNullOrEmptyMinimodTests
    {
        [Test]
        public void IsNullOrEmpty_NullString_IsTrue()
        {
            ((string)null).IsNullOrEmpty().Should().Be.True();
        }

        [Test]
        public void IsNullOrEmpty_EmptyString_IsTrue()
        {
            "".IsNullOrEmpty().Should().Be.True();
        }

        [Test]
        public void IsNullOrEmpty_A_IsFalse()
        {
            "A".IsNullOrEmpty().Should().Be.False();
        }

        [Test]
        public void IsNullOrEmpty_NullCollection_IsTrue()
        {
            ((ICollection)null).IsNullOrEmpty().Should().Be.True();
        }

        [Test]
        public void IsNullOrEmpty_EmptyCollectionImplementors_IsTrue()
        {
            new String[0].IsNullOrEmpty().Should().Be.True();
            new ArrayList().IsNullOrEmpty().Should().Be.True();
            new List<string>().IsNullOrEmpty().Should().Be.True();

            new Hashtable().IsNullOrEmpty().Should().Be.True();
            new Dictionary<string, string>().IsNullOrEmpty().Should().Be.True();
        }

        [Test]
        public void IsNullOrEmpty_FilledCollectionImplementors_IsTrue()
        {
            new []{"A"}.IsNullOrEmpty().Should().Be.False();
            new ArrayList { "A" }.IsNullOrEmpty().Should().Be.False();
            new List<string> { "A" }.IsNullOrEmpty().Should().Be.False();

            new Hashtable { {"A", "B"} }.IsNullOrEmpty().Should().Be.False();
            new Dictionary<string, string> { { "A", "B" } }.IsNullOrEmpty().Should().Be.False();
        }
    }
}