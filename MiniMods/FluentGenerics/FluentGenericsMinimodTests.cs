using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Minimod.JoinStrings;
using Minimod.PrettyTypeSignatures;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.FluentGenerics
{
    [TestFixture]
    public class FluentGenericsMinimodTests
    {
        [Test]
        public void PrintExamples()
        {
            printExample(typeof(List<int>), typeof(IEnumerable<>));
            printExample(typeof(Dictionary<int, string>), typeof(IDictionary<,>));
        }

        private static void printExample(Type type, Type genTypeDef)
        {
            bool isOfGenericType = type.IsOfGenericType(genTypeDef);

            Console.WriteLine("Is {0} an {1}? {2}",
                              type.GetPrettyName(),
                              genTypeDef.GetPrettyName(),
                              isOfGenericType ? "yes." : "no.");

            if (isOfGenericType)
            {
                Console.WriteLine("If it is, what are the type parameters? "
                                  + type.GetGenericArgumentsFor(genTypeDef)
                                        .Select(_ => _.GetPrettyName())
                                        .JoinStringsWith(", "));
            }
        }

        [Test]
        public void IsOfGenericType_ListOfInts_IsEnumerableOfInts()
        {
            typeof(List<int>).IsOfGenericType(typeof(IEnumerable<>))
                .Should().Be.True();
        }

        [Test]
        public void IsOfGenericType_ListOfInts_IsIListOfInts()
        {
            typeof(List<int>).IsOfGenericType(typeof(IList<>))
                .Should().Be.True();
        }

        [Test]
        public void IsOfGenericType_ListOfInts_IsNotGenericList()
        {
            typeof(IEnumerable<int>).IsOfGenericType(typeof(IList<>))
                .Should().Be.False();
        }

        [Test]
        public void GetGenericArgumentsFor_ListOfInts_ArgForIListShouldBeAnInt()
        {
            typeof(List<int>).GetGenericArgumentsFor(typeof(IList<>))
                .Should().Have.SameSequenceAs(typeof(int));
        }

        [Test]
        public void GetGenericTypeFor_ListOfInts_GenArgumentForIListIsInt()
        {
            typeof(List<int>).GetGenericTypeFor(typeof(IList<>))
                .IsGenericType.Should().Be.True();
        }
    }
}
