using System;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Minimod.MessageProcessor;

namespace Minimod.HttpMessageStream
{
    /// <summary>
    /// Minimod.HttpMessageStream, Version 0.0.1
    /// <para>Small embedded web server based on Rx and working like node.js.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class HttpMessageStream : IMessageStream
    {
        private readonly HttpListener _listener;
        private readonly IObservable<HttpContext> _stream;
        private readonly IScheduler _scheduler;

        public HttpMessageStream(string url)
            : this(url, new EventLoopScheduler())
        {

        }

        public HttpMessageStream(string url, IScheduler scheduler)
        {
            _scheduler = scheduler;
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _listener.Start();
            _stream = CreateObservableHttpContext();
        }

        private IObservable<HttpContext> CreateObservableHttpContext()
        {
            return Observable.Create<HttpContext>(obs =>
                Observable.FromAsyncPattern<HttpListenerContext>(_listener.BeginGetContext, _listener.EndGetContext)()
                          .Select(c => new HttpContext(c.Request, c.Response))
                          .Subscribe(obs))
                          .Repeat().Retry()
                          .Publish().RefCount().ObserveOn(_scheduler);
        }

        public void Dispose()
        {
            _listener.Stop();
        }

        public void Send<T>(T value)
        {
            throw new NotSupportedException();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _stream.Subscribe(observer);
        }
    }
}