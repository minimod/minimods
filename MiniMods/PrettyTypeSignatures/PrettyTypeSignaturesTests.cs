using System;
using System.Linq;
using System.Reflection;
using Minimod.PrettyText;
using NUnit.Framework;
using SharpTestsEx;

namespace Minimod.PrettyTypeSignatures
{
    [TestFixture]
    public class PrettyTypeSignaturesTests
    {
        [Test]
        public void ClassA_PrettyName()
        {
            typeof(ClassA).GetPrettyName().Should().Be.EqualTo("ClassA");
        }

        [Test]
        public void ClassXMethod2_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>).GetMethod("Method2");

            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>.Method2<T1>(int t, T1 hello) : bool");

            method.MakeGenericMethod(typeof(bool))
                .GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>.Method2<bool>(int t, bool hello) : bool");
        }

        [Test]
        public void ClassXMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>.Method(int t, bool hello) : bool");
        }

        [Test]
        public void ClassX_PrettyName()
        {
            Type type = typeof(ClassX<>);
            type.GetPrettyName().Should().Be.EqualTo("ClassX<T>");
        }

        [Test]
        public void ClassXofIntAndZ_PrettyName()
        {
            Type type = typeof(ClassX<int>.ClassZ);
            type.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassZ");
        }

        [Test]
        public void ClassXofInt_PrettyName()
        {
            Type type = typeof(ClassX<int>);
            type.GetPrettyName().Should().Be.EqualTo("ClassX<int>");
        }

        [Test]
        public void ClassYMethod2_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassY<string, string>).GetMethod("Method2");

            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassY<string,string>.Method2<T3>(string t1, T3 hello) : void");

            method.MakeGenericMethod(typeof(bool)).GetPrettyName()
                .Should().Be.EqualTo(
                                        "ClassX<int>+ClassY<string,string>.Method2<bool>(string t1, bool hello) : void");
        }

        [Test]
        public void ClassYMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassY<string, string>).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassY<string,string>.Method(string t1, bool hello) : void");
        }

        [Test]
        public void ClassY_PrettyName()
        {
            Type type = typeof(ClassX<>.ClassY<,>);
            type.GetPrettyName()
                .Should().Be.EqualTo("ClassX<T>+ClassY<T1,T2>");
        }

        [Test]
        public void ClassYofStringString_PrettyName()
        {
            Type type = typeof(ClassX<int>.ClassY<string, string>);
            type.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassY<string,string>");
        }

        [Test]
        public void ClassZMethod2_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassZ).GetMethod("Method2");

            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassZ.Method2<T1>() : T1");

            method.MakeGenericMethod(typeof(bool)).GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassZ.Method2<bool>() : bool");
        }

        [Test]
        public void ClassZMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassZ).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<int>+ClassZ.Method() : int");
        }

        [Test]
        public void ClassZ_PrettyName()
        {
            Type type = typeof(ClassX<>.ClassZ);
            type.GetPrettyName()
                .Should().Be.EqualTo("ClassX<T>+ClassZ");
        }

        [Test]
        public void EmptyAnonymous_PrettyName()
        {
            Type type = new { }.GetType();
            type.GetPrettyName()
                .Should().Be.EqualTo("Anonymous");
        }

        [Test]
        public void AnonymousWithProps_PrettyName()
        {
            Type type = new { StringProp = "", IntProp = 1 }.GetType();
            type.GetPrettyName()
                .Should().Be.EqualTo("Anonymous<string,int>");
        }

        [Test]
        public void CSharpBuiltinTypes_PrettyName()
        {
            typeof(int).GetPrettyName().Should().Be.EqualTo("int");
            typeof(void).GetPrettyName().Should().Be.EqualTo("void");
        }
    }
}