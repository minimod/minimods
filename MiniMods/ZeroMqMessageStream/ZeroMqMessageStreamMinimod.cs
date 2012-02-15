using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Minimod.MessageProcessor;
using Newtonsoft.Json;
using ZeroMQ;

namespace ConsoleApplication1.Minimods
{
    /// <summary>
    /// Minimod.ZeroMQMessageStream, Version 0.0.4
    /// <para>A minimod for messaging using ZeroMQ, Json and Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public interface IMessageStreamContext
    {
        void Send<T>(T value);
        void Send<T>(T message, params string[] publishAddresses);
    }

    public class MessageNotSendException : Exception
    {
        public string Reason { get; private set; }

        public MessageNotSendException(string reason)
            : base(reason)
        {
            Reason = reason;
        }
    }

    public class DocumentMessage : IMessage
    {
        public IMessageStreamContext MessageStreamContext { get; set; }
    }

    public class ZeroMqMessageStream : IMessageStreamContext, IDisposable
    {
        private readonly Subject<object> _stream;
        private readonly IDisposable _subscription;
        private readonly Guid _correlationId;
        private readonly ZmqContext _subContext;
        private readonly ZmqSocket _subSocket;
        private readonly ZmqContext _pubContext;
        private readonly ZmqSocket _pubSocket;
        private readonly IScheduler _scheduler;

        public IObservable<object> Stream
        {
            get
            {
                return _stream
                    .Select<object, dynamic>(jsonMessage => JsonConvert.DeserializeObject<ExpandoObject>(jsonMessage.ToString(), new Newtonsoft.Json.Converters.ExpandoObjectConverter()))
                    .Select<object, object>(message =>
                                                {
                                                    try
                                                    {
                                                        //resolve message type for deserializer
                                                        var messageTypeDescription = ((IDictionary<string, object>)message)
                                                                                            .Single(x => x.Key == "MessageTypeFullName")
                                                                                            .Value
                                                                                            .ToString();
                                                        var messageType = Type.GetType(messageTypeDescription);
                                                        ((IDictionary<string, object>)message).Remove("MessageTypeFullName");

                                                        //i don't need it yet
                                                        ((IDictionary<string, object>)message).Remove("CorrelationId");
                                                        ((IDictionary<string, object>)message).Remove("CorrelationTimeStamp");

                                                        //deserialize
                                                        var cleanedMessage = JsonConvert.SerializeObject(message);
                                                        var deserializeObject = JsonConvert.DeserializeObject(cleanedMessage, messageType, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Include });

                                                        //add message stream context if possible
                                                        if (deserializeObject.GetType().BaseType == typeof(DocumentMessage))
                                                            ((DocumentMessage)deserializeObject).MessageStreamContext = this;

                                                        return deserializeObject;
                                                    }
                                                    catch
                                                    {
                                                        return message;
                                                    }
                                                });
            }
        }

        public ZeroMqMessageStream(string subsciptionAddress)
            : this(new EventLoopScheduler(), subsciptionAddress)
        {
        }

        public ZeroMqMessageStream(string subsciptionAddress, params string[] publishAddresses)
            : this(new EventLoopScheduler(), subsciptionAddress, publishAddresses)
        {
        }

        public ZeroMqMessageStream(IScheduler scheduler, string subsciptionAddress, params string[] publishAddresses)
        {
            _scheduler = scheduler;
            _stream = new Subject<object>();
            _correlationId = Guid.NewGuid();
            _subContext = ZmqContext.Create();
            _subSocket = _subContext.CreateSocket(SocketType.SUB);
            _pubContext = ZmqContext.Create();
            _pubSocket = _pubContext.CreateSocket(SocketType.PUB);

            if (!String.IsNullOrEmpty(subsciptionAddress))
            {
                _subSocket.Bind(subsciptionAddress);
                _subSocket.Connect(subsciptionAddress);
                _subSocket.SubscribeAll();
            }

            ConnectToPublishers(_pubSocket, publishAddresses);

            _subscription = _scheduler.Schedule(() =>
                                                    {
                                                        while (true)
                                                        {
                                                            var zmqMessage = _subSocket.ReceiveMessage(TimeSpan.FromMilliseconds(100));
                                                            if (zmqMessage.FrameCount >= 0 && zmqMessage.TotalSize > 1 && zmqMessage.IsComplete)
                                                            {
                                                                var message = Encoding.UTF8.GetString(zmqMessage.First());
                                                                _stream.OnNext(message);
                                                                Debug.WriteLine("Correlation ID: {0} - message received with status {1}", _correlationId, _subSocket.ReceiveStatus);
                                                            }
                                                        }
                                                    });
        }

        public void Send<T>(T message)
        {
            SendInternal(message, _pubSocket);
        }

        public void Send<T>(T message, params string[] publishAddresses)
        {
            using (var pubContact = ZmqContext.Create())
            {
                using (var pubSocket = pubContact.CreateSocket(SocketType.PUB))
                {
                    ConnectToPublishers(pubSocket, publishAddresses);
                    SendInternal(message, pubSocket);
                }
            }
        }

        private void SendInternal<T>(T message, ZmqSocket socket)
        {
            var status = socket.SendFrame(new Frame(SerializeMessage(message)));
            if (status.ToString() != "Sent")
                throw new MessageNotSendException(status.ToString());
            Debug.WriteLine("Correlation ID: {0} - message was {1}", _correlationId, status);
        }

        private byte[] SerializeMessage<T>(T message)
        {
            var value = message as dynamic;

            //add message correlation information
            var serializeObject = JsonConvert.SerializeObject(value);
            var deserializeObject = JsonConvert.DeserializeObject<ExpandoObject>(serializeObject, new Newtonsoft.Json.Converters.ExpandoObjectConverter());
            deserializeObject.CorrelationTimeStamp = DateTime.Now;
            deserializeObject.CorrelationId = _correlationId.ToString();

            //information for deserializer
            deserializeObject.MessageTypeFullName = value.GetType().FullName;
            deserializeObject.MessageStreamContext = null;

            var serializedMessage = JsonConvert.SerializeObject(deserializeObject);
            return Encoding.UTF8.GetBytes(serializedMessage);
        }

        private void ConnectToPublishers(ZmqSocket socket, string[] publishAddresses)
        {
            foreach (var publishAddress in publishAddresses)
                socket.Connect(publishAddress);
            Thread.Sleep(publishAddresses.Count() * 2); //HACK: connect should be a blocking call
        }

        public void Dispose()
        {
            _stream.OnCompleted();
            _stream.Dispose();
            _subscription.Dispose();
            _subSocket.UnsubscribeAll();
            _pubSocket.Dispose();
            _pubContext.Dispose();
        }


        static readonly ConcurrentDictionary<string, ZeroMqMessageStream> Instance = new ConcurrentDictionary<string, ZeroMqMessageStream>();

        public static ZeroMqMessageStream GetDefault(string subscriptionAddress)
        {
            return Instance.GetOrAdd(subscriptionAddress, value => new ZeroMqMessageStream(subscriptionAddress));
        }
    }
}