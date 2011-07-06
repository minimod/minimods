using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Minimod.RxMessageBus
{
    /// <summary>
    /// Minimod.RxMessageBus, Version 0.0.1
    /// <para>A minimod event-/message bus.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class RxMessageBusMinimod
    {
        private static RxMessageBusMinimod _defaultInstance;
        private readonly Subject<object> _subject = new Subject<object>();

        public static RxMessageBusMinimod Default { get { return _defaultInstance ?? (_defaultInstance = new RxMessageBusMinimod()); } }
        public static void OverrideDefault(RxMessageBusMinimod newMessenger) { _defaultInstance = newMessenger; }
        public static void Reset() { _defaultInstance = null; }

        public IDisposable Register<T>(Action<T> action)
        {
            return _subject
                 .OfType<T>()
                 .Subscribe(action);
        }

        public IDisposable Register<T>(Action<T> action, IScheduler scheduler)
        {
            return _subject
                 .OfType<T>()
                 .ObserveOn(scheduler)
                 .Subscribe(action);
        }

        public IDisposable Register<T>(Action<T> action, Func<T, bool> predicate)
        {
            return Register(action, predicate, Scheduler.CurrentThread);
        }

        public IDisposable Register<T>(Action<T> action, Func<T, bool> predicate, IScheduler scheduler)
        {
            return _subject
                .OfType<T>()
                .ObserveOn(scheduler)
                .Where(predicate)
                .Subscribe(action);
        }


        public void Bridge<T>(Action<T> fromEvent)
        {
            Observable.FromEvent<T>(a => fromEvent += a, a => fromEvent -= a)
                .Select(x => (object)x)
                .Merge(_subject);
        }

        public IDisposable Register<T1, T2>(Action<Tuple<T1, T2>> action)
        {
            return Register(action, Scheduler.CurrentThread);
        }

        public IDisposable Register<T1, T2>(Action<Tuple<T1, T2>> action, IScheduler scheduler)
        {
            return Register<T1, T2, Tuple<T1, T2>>(action, (x, y) => new Tuple<T1, T2>(x, y), scheduler);
        }

        public IDisposable Register<T1, T2, TResult>(Action<TResult> action, Func<T1, T2, TResult> selector)
        {
            return Register(action, selector, Scheduler.CurrentThread);
        }

        public IDisposable Register<T1, T2, TResult>(Action<TResult> action, Func<T1, T2, TResult> selector, IScheduler scheduler)
        {
            var left = _subject.OfType<T1>();
            var right = _subject.OfType<T2>();
            var match = left
                .And(right)
                .Then(selector);
            return Observable
                .When(match)
                .ObserveOn(scheduler)
                .Subscribe(action);
        }


        public void Send<T>(T message)
        {
            _subject
                .OnNext(message);
        }
    }
}