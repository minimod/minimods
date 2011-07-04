namespace Minimod.PrettyText
{
    public class ClassX<T>
    {
        public bool Method(T t, bool hello)
        {
            return false;
        }

        public bool Method2<T1>(T t, T1 hello)
        {
            return false;
        }

        #region Nested type: ClassY

        public class ClassY<T1, T2>
        {
            public void Method(T1 t1, bool hello)
            {
            }

            public void Method2<T3>(T1 t1, T3 hello)
            {
            }
        }

        #endregion

        #region Nested type: ClassZ

        internal class ClassZ
        {
            public int Method()
            {
                return 0;
            }

            public T1 Method2<T1>()
            {
                return default(T1);
            }
        }

        #endregion
    }
}