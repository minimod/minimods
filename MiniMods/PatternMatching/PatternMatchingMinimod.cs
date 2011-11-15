using System;
using System.Collections.Generic;

namespace Minimod.PatternMatching
{
    /// <summary>
    /// Minimod.PatternMatching, Version 0.0.1
    /// <para>A minimod for easy to use pattern pattern matching in CSharp.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class MatchNotFoundException : Exception
    {
        public MatchNotFoundException(string message) : base(message) { }
    }
    public class PatternMatch<T, TResult>
    {
        private readonly T value;
        private readonly List<Tuple<Func<T, bool>, Func<T, TResult>>> cases = new List<Tuple<Func<T, bool>, Func<T, TResult>>>();
        private Func<T, TResult> elseFunc;

        internal PatternMatch(T value)
        {
            this.value = value;
        }

        public PatternMatch<T, TResult> With(Func<T, bool> condition, Func<T, TResult> result)
        {
            cases.Add(Tuple.Create(condition, result));
            return this;
        }

        public PatternMatch<T, TResult> With(Func<bool> condition, Func<T, TResult> result)
        {
            Func<T, bool> func = x => condition();
            cases.Add(Tuple.Create(func, result));
            return this;
        }


        public PatternMatch<T, TResult> Else(Func<T, TResult> result)
        {
            if (elseFunc != null)
                throw new InvalidOperationException("Cannot have multiple else cases");

            elseFunc = result;
            return this;
        }

        public TResult Do()
        {
            if (elseFunc != null)
                cases.Add(
                    Tuple.Create<Func<T, bool>, Func<T, TResult>>(x => true, elseFunc));
            foreach (var item in cases)
                if (item.Item1(value))
                    return item.Item2(value);

            throw new MatchNotFoundException("Incomplete pattern match");
        }
    }

    public class PatternMatchContext<T>
    {
        private readonly T _value;
        internal PatternMatchContext() { }
        internal PatternMatchContext(T value)
        {
            _value = value;
        }

        public PatternMatch<T, TResult> With<TResult>(Func<T, bool> condition, Func<T, TResult> result)
        {
            var match = new PatternMatch<T, TResult>(_value);
            return match.With(condition, result);
        }
        public PatternMatch<T, TResult> With<TResult>(Func<bool> condition, Func<T, TResult> result)
        {
            var match = new PatternMatch<T, TResult>(_value);
            return match.With(condition, result);
        }

    }

    public static class PatternMatchExtensions
    {
        public static PatternMatchContext<T> Match<T>(this T value)
        {
            return new PatternMatchContext<T>(value);
        }
    }
}
