using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Minimod.MessageProcessor
{
    /// <summary>
    /// Minimod.MessageProcessor, Version 0.0.1
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

        protected void On<T>(Func<IObservable<T>, IObservable<T>> action)
        {
            action(_subject.OfType<T>())
                .Subscribe();
        }
        public void Connect(IObservable<object> observable)
        {
            observable
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
                    MessageStream.Instance.Send(new ErrorMessage { Message = message as IMessage, Exception = error });
                }
                return result;
            };
        }
    }
    public class MyErrorHandler : MessageProcessor
    {
        public MyErrorHandler()
        {
            On<ErrorMessage>(messages => messages.Do(message => Debug.WriteLine(message.Exception.Message + " : " + message.Message)));
        }
    }
    public sealed class MessageStream
    {
        static readonly MessageStream _instance = new MessageStream();
        public static MessageStream Instance { get { return _instance; } }
        static MessageStream() { }
        readonly Subject<IMessage> _messageStream = new Subject<IMessage>();
        public IObservable<IMessage> Messages { get { return _messageStream; } }
        public void Send<T>(T value) where T : IMessage
        {
            _messageStream
                .OnNext(value);
        }
    }
}