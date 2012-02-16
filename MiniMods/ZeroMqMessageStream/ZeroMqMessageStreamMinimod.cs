using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Minimod.MessageProcessor;
using Newtonsoft.Json;
using ZeroMQ;

namespace Minimod.ZeroMqMessageStream
{
    /// <summary>
    /// Minimod.ZeroMQMessageStream, Version 0.0.4
    /// <para>A minimod for messaging using ZeroMQ, Json and Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public interface IMessageStreamContext : IMessageStream
    {
        void Send<T>(T values, params string[] publishAddresses);
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

    public class ZeroMqMessageStream : IObservable<object>, IMessageStreamContext, IDisposable
    {
        private readonly Subject<object> _messageStream = new Subject<object>();
        private readonly Guid _correlationId;
        private readonly ZmqContext _context;
        private readonly ZmqSocket _subSocket;
        private readonly ZmqSocket _pubSocket;
        private readonly IScheduler _scheduler;

        public ZeroMqMessageStream(params string[] publishAddresses)
            : this(new EventLoopScheduler(), String.Empty, publishAddresses)
        {

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
            _correlationId = Guid.NewGuid();
            _context = ZmqContext.Create();
            _subSocket = _context.CreateSocket(SocketType.SUB);
            _pubSocket = _context.CreateSocket(SocketType.PUB);

            if (!String.IsNullOrEmpty(subsciptionAddress))
            {
                _subSocket.Bind(subsciptionAddress);
                _subSocket.Connect(subsciptionAddress);
                _subSocket.SubscribeAll();
            }

            foreach (var publishAddress in publishAddresses)
                _pubSocket.Connect(publishAddress);

            Scheduler.NewThread.Schedule(() =>
                                             {
                                                 while (true)
                                                 {
                                                     var zmqMessage = _subSocket.ReceiveMessage(TimeSpan.FromMilliseconds(1000));
                                                     if (zmqMessage.FrameCount >= 0 && zmqMessage.TotalSize > 1 && zmqMessage.IsComplete)
                                                     {
                                                         var message = Encoding.UTF8.GetString(zmqMessage.First());
                                                         _messageStream.OnNext(message);
                                                         Debug.WriteLine("Correlation ID: {0} - message received with status {1}", _correlationId, _subSocket.ReceiveStatus);
                                                     }
                                                 }
                                             });
        }

        public void Send<T>(T value) where T : IMessage
        {
            SendInternal(value, _pubSocket);
        }
        public void Send<T>(T value, params string[] publishAddresses)
        {
            foreach (var publishAddress in publishAddresses)
                _pubSocket.Connect(publishAddress);
            SendInternal(value, _pubSocket);
        }
        private void SendInternal<T>(T value, ZmqSocket socket)
        {
            var status = socket.SendFrame(new Frame(SerializeMessage(value)));
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
        public void Dispose()
        {
            _messageStream.OnCompleted();
            _subSocket.Close();
            _pubSocket.Close();
            _context.Terminate();
            _messageStream.Dispose();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _messageStream
                .SubscribeOn(_scheduler)
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
                                                catch (Exception error)
                                                {
                                                    _messageStream.OnError(error);
                                                }
                                                return message;
                                            })
                .Subscribe(observer);
        }
    }
}