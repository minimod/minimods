using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ZMQ;

namespace Minimod.ZeroMQMessageBus
{
    /// <summary>
    /// Minimod.ZeroMQMessageBus, Version 0.0.1
    /// <para>A minimod for messaging using ZeroMQ, Json and Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class ZeroMQMessageBusMinimod : IDisposable
    {
        private readonly Context _subContext;
        private readonly Context _pubContext;
        private readonly Socket _subSocket;
        private readonly Socket _pubSocket;
        private readonly Subject<object> _stream;
        private IDisposable _subscription;
        private readonly Guid _correlationId;

        private static ZeroMQMessageBusMinimod _defaultInstance;
        public static ZeroMQMessageBusMinimod Default { get { return _defaultInstance ?? (_defaultInstance = new ZeroMQMessageBusMinimod()); } }
        public static void OverrideDefault(ZeroMQMessageBusMinimod newMessenger) { _defaultInstance = newMessenger; }
        public static void Reset() { _defaultInstance = null; }
        public IObservable<object> Stream { get { return _stream; } }

        public ZeroMQMessageBusMinimod()
        {
            _correlationId = Guid.NewGuid();
            _subContext = new Context(1);
            _pubContext = new Context(1);
            _stream = new Subject<object>();
            _subSocket = _subContext.Socket(SocketType.SUB);
            _pubSocket = _pubContext.Socket(SocketType.PUB);
        }

        public void Connect(string subsciptionAddress, string publishAddress)
        {
            _subSocket.Connect(subsciptionAddress);
            _pubSocket.Connect(publishAddress);

            _subSocket.Subscribe(String.Empty, Encoding.UTF8);

            _subscription = Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => _stream.OnNext(_subSocket.Recv(Encoding.UTF8)));

        }

        public IDisposable Register<T>(Action<T> action)
        {
            return Register(action, new SynchronizationContextScheduler(SynchronizationContext.Current));
        }

        public IDisposable Register<T>(Action<T> action, IScheduler scheduler)
        {
            return _stream
                .Where(message => !message.ToString().Contains(_correlationId.ToString()))
                .Select<object, dynamic>(jsonMessage => JsonConvert.DeserializeObject<ExpandoObject>(jsonMessage.ToString(), new Newtonsoft.Json.Converters.ExpandoObjectConverter()))                
                .Select(message =>
                            {
                                try
                                {
                                    ((IDictionary<string, object>)message).Remove("CorrelationId");
                                    ((IDictionary<string, object>)message).Remove("CorrelationTimeStamp");
                                    var serializeObject = JsonConvert.SerializeObject(message);
                                    return JsonConvert.DeserializeObject<T>(serializeObject, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });
                                }
                                catch { return message; }
                            })
                .OfType<T>()
                .ObserveOn(scheduler)
                .Subscribe(action);
        }

        public void Send(dynamic value)
        {
            var serializeObject = JsonConvert.SerializeObject(value);
            var deserializeObject = JsonConvert.DeserializeObject<ExpandoObject>(serializeObject, new Newtonsoft.Json.Converters.ExpandoObjectConverter());
            deserializeObject.CorrelationTimeStamp = DateTime.Now;
            deserializeObject.CorrelationId = _correlationId.ToString();
            var message = JsonConvert.SerializeObject(deserializeObject);
            _pubSocket.Send(message, Encoding.UTF8);
        }

        public void Dispose()
        {
            _stream.OnCompleted();
            _subscription.Dispose();
            _pubSocket.Dispose();
            _subSocket.Dispose();
            _subContext.Dispose();
        }
    }
}