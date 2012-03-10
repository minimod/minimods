using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Minimod.HttpMessageStream.Utils;
using Minimod.MessageProcessor;

namespace Minimod.HttpMessageStream
{    
    public class HttpContext : IMessage
    {
        public bool TryHandleRoute(string uri)
        {
            UriTemplateMatch uriTemplateMatch;
            return TryUriTemplateMatch(uri, out uriTemplateMatch);
        }

        private bool TryUriTemplateMatch(string uri, out UriTemplateMatch uriTemplateMatch)
        {
            var uriTemplate = new UriTemplate(uri);
            var serverPath = Request.Url.GetServerBaseUri();
            uriTemplateMatch = uriTemplate.Match(new Uri(serverPath), Request.Url);
            return uriTemplateMatch != null;
        }

        public readonly HttpListenerRequest ListenerRequest;
        public readonly HttpListenerResponse ListenerResponse;
        public virtual Request Request { get; private set; }

        public HttpContext(HttpListenerRequest request, HttpListenerResponse response)
        {
            ListenerRequest = request;
            ListenerResponse = response;
            Request = new Request
            {
                ContentEncoding = request.ContentEncoding,
                Headers = request.Headers.AllKeys.ToDictionary<string, string, IEnumerable<string>>(key => key, request.Headers.GetValues),
                HttpMethod = request.HttpMethod,
                InputStream = request.InputStream,
                Url = request.Url
            };
        }

        public void MapRequestUriArgumentsFromUri(string uri)
        {
            UriTemplateMatch uriTemplateMatch;
            if (String.IsNullOrEmpty(uri)) return;
            if (!TryUriTemplateMatch(uri, out uriTemplateMatch)) return;

            Request.UriArguments = new ArgumentsToDynamic(uriTemplateMatch.BoundVariables);
        }

        public Func<Result> ExecuteResult = () => new EmptyHtmlTextResult(404);

        public virtual void Result(Func<Result> action)
        {
            ExecuteResult = action;
        }
    }
    public class HttpPost : HttpContext
    {
        public HttpPost(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {

        }
    }
    public class HttpGet : HttpContext
    {
        public HttpGet(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
    public class HttpPut : HttpContext
    {
        public HttpPut(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
    public class HttpDelete : HttpContext
    {
        public HttpDelete(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
    public class HttpHead : HttpContext
    {
        public HttpHead(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
    public class HttpTrace : HttpContext
    {
        public HttpTrace(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
    public class HttpRaw : HttpContext
    {
        public HttpRaw(HttpListenerRequest request, HttpListenerResponse response)
            : base(request, response)
        {
        }
    }
}