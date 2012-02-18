using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Fleck;
using Minimod.MessageProcessor;

namespace Minimod.WebSocketMessageStream
{
    /// <summary>
    /// Minimod.WebSocketMessageStream, Version 0.0.3
    /// <para>A minimod for messaging using HTML5 WebSockets.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class WebSocketMessageStream : IObservable<object>, IMessageStream, IDisposable
    {
        readonly IScheduler _scheduler;
        private readonly Guid _correlationId;
        readonly Subject<object> _stream = new Subject<object>();
        readonly WebSocketServer _server;
        readonly List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();

        public IObservable<object> Stream
        {
            get { return _stream; }
        }

        internal IWebSocketConnection[] AllSockets
        {
            get { return _allSockets.ToArray(); }
        }

        public WebSocketMessageStream(string subsciptionAddress)
            : this(subsciptionAddress, new EventLoopScheduler())
        {

        }

        public WebSocketMessageStream(string subsciptionAddress, IScheduler scheduler)
        {
            _scheduler = scheduler;
            _correlationId = Guid.NewGuid();
            _server = new WebSocketServer("ws://" + subsciptionAddress);
            _server.Start(socket =>
            {
                socket.OnOpen = () => _allSockets.Add(socket);
                socket.OnClose = () => _allSockets.Remove(socket);
                socket.OnError = error =>
                {
                    _scheduler.Schedule(() => _stream.OnError(error));
                    Debug.WriteLine("Correlation ID: {0} - message receive error {1}", _correlationId, error.Message);
                };
                socket.OnMessage = message =>
                {
                    _scheduler.Schedule(() => _stream.OnNext(message));
                    Debug.WriteLine("Correlation ID: {0} - message {1} received from {2}", _correlationId, message, socket.ConnectionInfo);
                };
            });
        }

        public void Send<T>(T value)
        {
            InternalSend(value);
        }


        void InternalSend<T>(T value)
        {
            AllSockets.ForEach(socket =>
            {
                socket.Send(value.ToString());
                Debug.WriteLine("Message {0} send to {1}.", value, socket.ConnectionInfo);
            });
        }

        public void Dispose()
        {
            _stream.OnCompleted();
            _server.Dispose();
            _stream.Dispose();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _stream.Subscribe(observer);
        }
    }
}