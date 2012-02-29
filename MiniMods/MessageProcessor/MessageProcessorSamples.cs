using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace Minimod.MessageProcessor
{
    public class MessageProcessorSamples
    {
        public class MyMessage : IMessage
        {
            public string Reason { get; set; }
            public bool IsFirst { get; set; }
            public bool IsSecond { get; set; }
        }
        public class MyMessageProcessor : MessageProcessor
        {
            public MyMessageProcessor()
                : base(MessageStream.GetMain())
            {
                OnReceive<MyMessage>(messages => messages
                                              .Where(message => message.IsSecond)
                                              .Do(message => WhenIsSecond(message)));

                OnReceive<MyMessage>(messages => messages
                                              .Where(message => message.IsFirst)
                                              .Do(message => WhenIsFirst(message)));
            }

            Unit WhenIsFirst(MyMessage message)
            {
                //throw new Exception("MY ERROR!");
                Console.WriteLine("First - " + message.Reason + " ThreadId: " + Thread.CurrentThread.ManagedThreadId);
                return Unit.Default;
            }
            Unit WhenIsSecond(MyMessage message)
            {
                Console.WriteLine("Second:" + message.Reason + " ThreadId: " + Thread.CurrentThread.ManagedThreadId);
                return Unit.Default;
            }
        }

        public void Sample()
        {
            new ErrorProcessor();
            new MyMessageProcessor();

            Observable
                .Generate(0, x => x < 100, x => x + 1, x => new MyMessage { IsFirst = x % 2 == 0, IsSecond = x % 2 != 0, Reason = x.ToString() })
                .Subscribe(x => MessageStream.GetMain().Send(x));
        }

    }
}