using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Minimod.MessageProcessor
{
    /// <summary>
    /// Minimod.MessageProcessor, Version 0.0.6
    /// <para>A processor for messages.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public interface IMessage { }
    public interface IMessageStream : IObservable<object>, IDisposable
    {
        void Send<T>(T value);
    }

    public abstract class MessageProcessor : IDisposable
    {
        protected readonly Subject<object> Stream = new Subject<object>();
        private readonly IDisposable _streamSubscription;

        protected void OnReceive<T>(Func<IObservable<T>, IObservable<T>> action)
        {
            action(Stream.OfType<T>()).Subscribe();
        }

        protected MessageProcessor(IObservable<object> messages)
        {
            _streamSubscription = messages.Subscribe(Stream);
        }

        public virtual void Dispose()
        {
            _streamSubscription.Dispose();
        }
    }

    public sealed class MessageStream : IMessageStream
    {
        readonly string _name;
        readonly IScheduler _scheduler;
        readonly Subject<object> _messageStream = new Subject<object>();
        readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        MessageStream(string name, IScheduler scheduler)
        {
            _name = name;
            _scheduler = scheduler;
        }

        public void Send<T>(T value)
        {
            var currentStream = MessageStreams.Single(x => x.Key == _name);
            _scheduler.Schedule(() => currentStream
                                      .Value
                                      ._messageStream
                                      .OnNext(value));
        }
        public IDisposable Subscribe(IObserver<object> observer)
        {
            var subscription = _messageStream.Subscribe(observer);
            _subscriptions.Add(subscription);
            return subscription;
        }

        static readonly ConcurrentDictionary<string, MessageStream> MessageStreams = new ConcurrentDictionary<string, MessageStream>();
        public static MessageStream CreateLabeled(string name, IScheduler scheduler)
        {
            return MessageStreams.GetOrAdd(name.ToLower(), value => new MessageStream(value, scheduler));
        }
        public static MessageStream GetSequential(string name)
        {
            return CreateLabeled(name + "::serial::", new EventLoopScheduler());
        }
        public static MessageStream GetConcurrent(string name)
        {
            return CreateLabeled(name + "::concurrent::", Scheduler.ThreadPool);
        }
        public static MessageStream GetMain()
        {
#if SILVERLIGHT
            return CreateLabeled("::main::", DispatcherScheduler.Instance); //install-package rx-xaml
#endif
            return CreateLabeled("::main::", Scheduler.CurrentThread);
        }
        public static MessageStream GetGlobal()
        {
            return CreateLabeled("::global::", new EventLoopScheduler());
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }

    public class ErrorMessage : IMessage
    {
        public IMessage Message { get; set; }
        public Exception Exception { get; set; }
    }
    public class ErrorProcessor : MessageProcessor
    {
        public ErrorProcessor()
            : base(MessageStream.GetMain())
        {
            OnReceive<ErrorMessage>(messages => messages.Do(message => Debug.WriteLine(message.Exception.Message + " : " + message.Message)));
        }
    }
}