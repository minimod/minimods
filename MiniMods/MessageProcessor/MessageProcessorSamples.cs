using System;
using System.Diagnostics;
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
                On<MyMessage>(messages => messages
                                              .Where(message => message.IsSecond)
                                              .Do(
                                                  message => TryCatch(Log(Log(Log(Log<MyMessage>(WhenIsSecond)))))(message)));

                On<MyMessage>(messages => messages
                                              .Where(message => message.IsFirst)
                                              .Do(message => TryCatch(Log<MyMessage>(WhenIsFirst))(message)));
            }

            /// <summary>
            /// Handles message.
            /// </summary>
            /// <param name="message">Message to handle.</param>
            /// <returns>void</returns>
            Unit WhenIsFirst(MyMessage message)
            {
                //throw new Exception("MY ERROR!");
                Console.WriteLine("First - " + message.Reason + " ThreadId: " + Thread.CurrentThread.ManagedThreadId);
                return Unit.Default;
            }
            /// <summary>
            /// Handles message.
            /// </summary>
            /// <param name="message">Message to handle.</param>
            /// <returns></returns>
            Unit WhenIsSecond(MyMessage message)
            {
                // handle event
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