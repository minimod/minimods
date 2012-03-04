using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Minimod.MessageProcessor;

namespace Minimod.HttpMessageStream
{
    public class ServerErrorMessage : IMessage
    {
        public string Message { get; set; }
        public HttpContext HttpContext { get; set; }
    }

    public class RouteFoundMessage : IMessage
    {
        public string Uri { get; set; }
    }

    /// <summary>
    /// Minimod.HttpMessageStream, Version 0.0.5
    /// <para>Small embedded web server based on Rx and working like node.js.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public abstract class HttpMessageProcessor : MessageProcessor.MessageProcessor
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

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
                .Do(httpContext => Stream.OnNext(new RouteFoundMessage() { Uri = uri }))
                .OfType<T>();

            _subscriptions.Add(action(contextPipe)
                                    .OfType<HttpContext>()
                                    .Do(httpContext =>
                                    {
                                        try { httpContext.ExecuteResult().Send(httpContext.ListenerResponse); }
                                        catch (Exception error)
                                        {
                                            var message = error.Message;
                                            if (error.InnerException != null)
                                                message += " -> " + error.InnerException.Message;

                                            Stream.OnNext(new ServerErrorMessage { Message = message, HttpContext = httpContext });
                                        }
                                    })
                                    .Retry()
                                    .Subscribe());
        }

        protected HttpMessageProcessor(IObservable<object> messages)
            : base(messages)
        {
            _subscriptions.Add(Handle500ServerError().Subscribe());
            _subscriptions.Add(Handle404NotFound().Subscribe());
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscriptions.Dispose();
        }

        private IObservable<object> Handle404NotFound()
        {
            return Stream
                .OfType<HttpContext>()
                .TakeUntil(Stream.OfType<RouteFoundMessage>())
                .Repeat()
                .SkipUntil(Stream)
                .Do(httpContext => httpContext.Result(() => new EmptyHtmlTextResult(404)))
                .Do(httpContext => httpContext.ExecuteResult().Send(httpContext.ListenerResponse));
        }

        private IObservable<object> Handle500ServerError()
        {
            return Stream.OfType<ServerErrorMessage>()
                .Do(message => message.HttpContext.Result(() => new StringHtmlTextResult(message.Message, 500)))
                .Do(message => message.HttpContext.ExecuteResult().Send(message.HttpContext.ListenerResponse));
        }
    }
}