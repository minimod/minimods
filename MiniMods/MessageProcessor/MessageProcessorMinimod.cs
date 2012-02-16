using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Minimod.MessageProcessor
{
    /// <summary>
    /// Minimod.MessageProcessor, Version 0.0.5
    /// <para>A processor for messages.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public interface IMessage { }
    public class ErrorMessage : IMessage
    {
        public IMessage Message { get; set; }
        public Exception Exception { get; set; }
    }

    public abstract class MessageProcessor
    {
        readonly Subject<object> _subject = new Subject<object>();
        protected void OnReceive<T>(Func<IObservable<T>, IObservable<T>> action)
        {
            action(_subject.OfType<T>()).Subscribe();
        }

        protected MessageProcessor(IObservable<object> messages)
        {
            messages
                .Multicast(_subject)
                .Connect();
        }

        protected Func<T, Unit> Log<T>(Func<T, Unit> next)
        {
            return _ =>
            {
                Debug.WriteLine("Entry method: " + next.Method.Name);
                var result = next(_);
                Debug.WriteLine("Result method: " + next.Method.Name);
                return result;
            };
        }
        protected Func<T, Unit> TryCatch<T>(Func<T, Unit> next)
        {
            return message =>
            {
                var result = Unit.Default;
                try
                {
                    result = next(message);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error.Message);
                    MessageStream.GetMain().Send(new ErrorMessage { Message = message as IMessage, Exception = error });
                }
                return result;
            };
        }
    }
    public class ErrorProcessor : MessageProcessor
    {
        public ErrorProcessor()
            : base(MessageStream.GetMain())
        {
            OnReceive<ErrorMessage>(messages => messages.Do(message => Debug.WriteLine(message.Exception.Message + " : " + message.Message)));
        }
    }

    public interface IMessageStream
    {
        void Send<T>(T value);
    }
    public sealed class MessageStream : IObservable<object>, IMessageStream
    {
        readonly string _name;
        readonly IScheduler _scheduler;
        readonly Subject<object> _messageStream = new Subject<object>();

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

        static readonly ConcurrentDictionary<string, MessageStream> MessageStreams = new ConcurrentDictionary<string, MessageStream>();
        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _messageStream.Subscribe(observer);
        }

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
    }
}