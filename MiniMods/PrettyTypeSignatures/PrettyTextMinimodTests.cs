using System;
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
                .Should().Be.EqualTo("ClassX<Int32>.Method2<T1>(Int32 t, T1 hello) : Boolean");

            method.MakeGenericMethod(typeof(bool))
                .GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>.Method2<Boolean>(Int32 t, Boolean hello) : Boolean");
        }

        [Test]
        public void ClassXMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>.Method(Int32 t, Boolean hello) : Boolean");
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
                .Should().Be.EqualTo("ClassX<Int32>+ClassZ");
        }

        [Test]
        public void ClassXofInt_PrettyName()
        {
            Type type = typeof(ClassX<int>);
            type.GetPrettyName().Should().Be.EqualTo("ClassX<Int32>");
        }

        [Test]
        public void ClassYMethod2_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassY<string, string>).GetMethod("Method2");

            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>+ClassY<String,String>.Method2<T3>(String t1, T3 hello) : Void");

            method.MakeGenericMethod(typeof(bool)).GetPrettyName()
                .Should().Be.EqualTo(
                                        "ClassX<Int32>+ClassY<String,String>.Method2<Boolean>(String t1, Boolean hello) : Void");
        }

        [Test]
        public void ClassYMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassY<string, string>).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>+ClassY<String,String>.Method(String t1, Boolean hello) : Void");
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
                .Should().Be.EqualTo("ClassX<Int32>+ClassY<String,String>");
        }

        [Test]
        public void ClassZMethod2_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassZ).GetMethod("Method2");

            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>+ClassZ.Method2<T1>() : T1");

            method.MakeGenericMethod(typeof(bool)).GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>+ClassZ.Method2<Boolean>() : Boolean");
        }

        [Test]
        public void ClassZMethod_PrettyName()
        {
            MethodInfo method = typeof(ClassX<int>.ClassZ).GetMethod("Method");
            method.GetPrettyName()
                .Should().Be.EqualTo("ClassX<Int32>+ClassZ.Method() : Int32");
        }

        [Test]
        public void ClassZ_PrettyName()
        {
            Type type = typeof(ClassX<>.ClassZ);
            type.GetPrettyName()
                .Should().Be.EqualTo("ClassX<T>+ClassZ");
        }
    }
}