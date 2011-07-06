using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Minimod.RxMessageBus
{
    public class RxMessageBus
    {
        private static RxMessageBus _defaultInstance;
        private readonly Subject<object> _subject = new Subject<object>();

        public static RxMessageBus Default { get { return _defaultInstance ?? (_defaultInstance = new RxMessageBus()); } }
        public static void OverrideDefault(RxMessageBus newMessenger) { _defaultInstance = newMessenger; }
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