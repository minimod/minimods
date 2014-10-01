using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace Minimod.RabbitMqTopicsMessageStream
{
    /// <summary>
    /// Minimod.RabbitMqTopicsMessageStream, Version 0.0.1
    /// <para>A minimod for messaging using RabbitMQ, Json and Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal class RabbitMqTopicsMessageStream
    {
        private readonly ReplaySubject<object> _messageStream = new ReplaySubject<object>();
        private readonly IScheduler _scheduler;
        readonly IConnection _mqConnection;
        readonly IModel _mqChannel;
        readonly Subscription _subscription;
        const string ExchangeName = "amq.topic";
        private bool _disposed;
        readonly IDisposable _receiver;

        public Func<object, string> Serializer { get; set; }
        public Func<string, Type, object> Deserializer { get; set; }
        public Func<Type, string> RoutingResolver { get; set; }

        public RabbitMqTopicsMessageStream(string uri, string queueName)
            : this(new EventLoopScheduler(), uri, queueName)
        {
        }

        public RabbitMqTopicsMessageStream(IScheduler scheduler, string uri, string queueName)
        {
            _scheduler = scheduler;
            var mqConnection = new ConnectionFactory { Uri = uri };
            _mqConnection = mqConnection.CreateConnection();
            _mqChannel = _mqConnection.CreateModel();
            _mqChannel.QueueDeclare(queueName, true, false, false, null);
            _subscription = new Subscription(_mqChannel, queueName, false);

            _receiver = Scheduler.NewThread.Schedule(() =>
            {
                while (!_disposed)
                {
                    var message = _subscription.Next();
                    if (message != null) _messageStream.OnNext(message);
                }
            });
        }

        public void Send<T>(T value)
        {
            var msg = Serializer(value);
            _mqChannel.BasicPublish(ExchangeName, typeof(T).Name, null, Encoding.UTF8.GetBytes(msg));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _receiver.Dispose();
                    _subscription.Close();
                    _mqChannel.Close();
                    _mqConnection.Close();
                    _messageStream.OnCompleted();
                    _messageStream.Dispose();
                }
                _disposed = true;
            }
        }
        public IDisposable Subscribe<T>(Action<T> onNext, Action<Exception> onError)
        {
            return _messageStream
                .SubscribeOn(_scheduler)
                .OfType<BasicDeliverEventArgs>()
                .Where(message => RoutingResolver(typeof(T)) == message.RoutingKey)
                .Do(message =>
                {
                    try
                    {
                        var result = (T)Deserializer(Encoding.UTF8.GetString(message.Body), typeof(T));
                        onNext(result);
                        _subscription.Ack(message);
                    }
                    catch (Exception error) { onError(error); }
                })
                .OfType<T>()
                .Subscribe();
        }
    }

}