using System;
using System.Security.Authentication;
using System.Threading;
using NUnit.Framework;

namespace Minimod.ThreadVariable
{
    [TestFixture]
    public class RealWorldExample
    {
        [Test]
        public void TryDeleteDataBase()
        {
            typeof(AccessViolationException).ShouldBeThrownBy(() => BusinessObject.DeleteDataBase());
        }

        [Test]
        public void DeleteDataBaseAsSuperUser()
        {
            using (UserContext.EnterSuperUserScope("supersecure"))
            {
                BusinessObject.DeleteDataBase(); // works perfectly
            }
        }

        [Test]
        public void DeleteDataBaseNestedAsSuperUser()
        {
            using (UserContext.EnterSuperUserScope("supersecure"))
            {
                BusinessObject.DeleteDataBase(); // works perfectly
                using (UserContext.EnterSuperUserScope("supersecure"))
                {
                    BusinessObject.DeleteDataBase(); // works perfectly
                }
                BusinessObject.DeleteDataBase(); // works perfectly
            }
        }

        [Test]
        public void DeleteDataBaseAsSuperUserRetryAfterwards()
        {
            using (UserContext.EnterSuperUserScope("supersecure"))
            {
                BusinessObject.DeleteDataBase(); // works perfectly
            }

            typeof(AccessViolationException).ShouldBeThrownBy(() => BusinessObject.DeleteDataBase());
        }

        [Test]
        public void UsingPublicThreadVar()
        {
            SomeThreadVars.IsSuperUser.Current.ShouldBeFalse();
            using (SomeThreadVars.IsSuperUser.Use(true))
            {
                SomeThreadVars.IsSuperUser.Current.ShouldBeTrue();
            }
            SomeThreadVars.IsSuperUser.Current.ShouldBeFalse();
        }
    }

    public class UserContext
    {
        private static readonly ThreadVariable<bool> isSuperUser = new ThreadVariable<bool>(false);

        public static bool IsSuperUser
        {
            get { return isSuperUser.Current; }
        }

        public static IDisposable EnterSuperUserScope(string passkey)
        {
            if (passkey != "supersecure")
                throw new InvalidCredentialException("The passkey you used is wrong!");

            return isSuperUser.Use(true);
        }
    }

    public class BusinessObject
    {
        public static bool DeleteDataBase()
        {
            if (!UserContext.IsSuperUser && !Thread.CurrentPrincipal.IsInRole("canDeleteDataBase"))
                throw new AccessViolationException("go home!");

            // delete the database
            return true;
        }
    }

    internal static class SomeThreadVars
    {
        internal static readonly ThreadVariable<bool> IsSuperUser = new ThreadVariable<bool>(false);
    }
}