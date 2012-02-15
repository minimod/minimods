using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Newtonsoft.Json;
using ZeroMQ;

namespace Minimod.ZeroMqMessageStream
{
    /// <summary>
    /// Minimod.ZeroMqMessageStream, Version 0.0.2
    /// <para>A minimod for messaging using ZeroMQ, Json and Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class ZeroMqMessageStream : IDisposable
    {
        private readonly Subject<object> _stream;
        private IDisposable _subscription;
        private readonly Guid _correlationId;

        private readonly ZmqContext _pubContext;
        private readonly ZmqContext _subContext;
        private readonly ZmqSocket _pubSocket;
        private readonly ZmqSocket _subSocket;
        private readonly IScheduler _scheduler;

        public IObservable<object> Stream
        {
            get
            {
                return _stream
                    .Select<object, dynamic>(jsonMessage => JsonConvert.DeserializeObject<ExpandoObject>(jsonMessage.ToString(), new Newtonsoft.Json.Converters.ExpandoObjectConverter()))
                    .Select(message =>
                                {
                                    try
                                    {
                                        ((IDictionary<string, object>)message).Remove("CorrelationId");
                                        ((IDictionary<string, object>)message).Remove("CorrelationTimeStamp");
                                        var serializeObject = JsonConvert.SerializeObject(message);
                                        return JsonConvert.DeserializeObject<object>(serializeObject, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });
                                    }
                                    catch
                                    {
                                        return message;
                                    }
                                });

            }
        }

        public ZeroMqMessageStream()
            : this(new EventLoopScheduler())
        {

        }

        public ZeroMqMessageStream(IScheduler scheduler)
        {
            _scheduler = scheduler;
            _stream = new Subject<object>();
            _correlationId = Guid.NewGuid();
            _pubContext = ZmqContext.Create();
            _subContext = ZmqContext.Create();
            _pubSocket = _pubContext.CreateSocket(SocketType.PUB);
            _subSocket = _subContext.CreateSocket(SocketType.SUB);
        }

        public void Connect(string subsciptionAddress, string[] publishAddresses)
        {
            foreach (var publishAddress in publishAddresses)
            {
                _pubSocket.Connect(publishAddress);
            }

            _subSocket.Bind(subsciptionAddress);
            _subSocket.Connect(subsciptionAddress);
            _subSocket.SubscribeAll();

            _subscription = _scheduler.Schedule(() =>
                                    {
                                        while (true)
                                        {
                                            var zmqMessage = _subSocket.ReceiveMessage();
                                            if (zmqMessage.FrameCount >= 0 && zmqMessage.TotalSize > 1 && zmqMessage.IsComplete)
                                            {
                                                var message = Encoding.UTF8.GetString(zmqMessage.First());
                                                _stream.OnNext(message);
                                                Debug.WriteLine("Correlation ID: {0} - message received", _correlationId);
                                            }
                                        }
                                    });
        }

        public void Send(dynamic value)
        {
            var serializeObject = JsonConvert.SerializeObject(value);
            var deserializeObject = JsonConvert.DeserializeObject<ExpandoObject>(serializeObject, new Newtonsoft.Json.Converters.ExpandoObjectConverter());
            deserializeObject.CorrelationTimeStamp = DateTime.Now;
            deserializeObject.CorrelationId = _correlationId.ToString();
            var message = JsonConvert.SerializeObject(deserializeObject);
            _pubSocket.SendFrame(new Frame(Encoding.UTF8.GetBytes(message)));
            Debug.WriteLine("Correlation ID: {0} - message send", _correlationId);
        }

        public void Dispose()
        {
            _stream.OnCompleted();
            _subscription.Dispose();
            _pubSocket.Dispose();
            _subSocket.Dispose();
            _subContext.Dispose();
            _pubContext.Dispose();
        }
    }
}