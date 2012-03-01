using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Minimod.MessageProcessor;

namespace Minimod.HttpMessageStream
{
    /// <summary>
    /// Minimod.HttpMessageStream, Version 0.0.3
    /// <para>Small embedded web server based on Rx and working like node.js.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public abstract class HttpMessageProcessor : MessageProcessor.MessageProcessor
    {
        private readonly Subject<object> _routeFoundStream = new Subject<object>();
        private readonly IDisposable _404Subscription;

        public new void OnReceive<T>(Func<IObservable<T>, IObservable<T>> action)
        {
            OnReceive("/", (Func<IObservable<HttpGet>, IObservable<HttpGet>>) action);
        }

        public void OnReceive<T>(string uri, Func<IObservable<T>, IObservable<T>> action)
            where T : class, IMessage
        {
            var contextPipe = Stream
                .OfType<HttpContext>()
                .Where(httpContext => httpContext.TryHandleRoute(uri))
                .Select(httpContext =>
                            {
                                switch (httpContext.Request.HttpMethod)
                                {
                                    case "GET":
                                        return new HttpGet(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "POST":
                                        return new HttpPost(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "PUT":
                                        return new HttpPut(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "DELETE":
                                        return new HttpDelete(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "HEAD":
                                        return new HttpHead(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "OPTIONS":
                                        return new HttpOptions(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    case "TRACE":
                                        return new HttpTrace(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                    default:
                                        return new HttpRaw(httpContext.ListenerRequest, httpContext.ListenerResponse) as T;
                                }
                            })
                .OfType<HttpContext>()
                .Do(httpContext => httpContext.MapRequestUriArgumentsFromUri(uri))
                .Do(httpContext => _routeFoundStream.OnNext(httpContext))
                .OfType<T>();

            action(contextPipe)
                .OfType<HttpContext>()                
                .Do(httpContext => httpContext.ExecuteResult().Send(httpContext.ListenerResponse))
                .Retry()
                .Subscribe();
        }

        protected HttpMessageProcessor(IObservable<object> messages)
            : base(messages)
        {
            _404Subscription = Handle404NotFound()
                .Subscribe();
        }

        public override void Dispose()
        {
            base.Dispose();
            _404Subscription.Dispose();
        }

        private IObservable<object> Handle404NotFound()
        {
            return Stream
              .OfType<HttpContext>()
              .TakeUntil(_routeFoundStream)
              .Repeat()
              .SkipUntil(Stream)
              .Do(httpContext => httpContext.Result(() => new EmptyHtmlTextResult(404)))
              .Do(httpContext => httpContext.ExecuteResult().Send(httpContext.ListenerResponse));
        }
    }
}