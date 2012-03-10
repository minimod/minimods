using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;

namespace Minimod.HttpMessageStream
{
    public abstract class Result
    {
        public int StatusCode { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        protected Result(int statusCode = 200)
        {
            Headers = new Dictionary<string, string>();
            StatusCode = statusCode;
        }

        protected virtual IObservable<Stream> WriteStream(Stream stream)
        {
            return Observable.Return(stream);
        }

        public void Send(HttpListenerResponse listenerResponse)
        {
            Headers
                .Where(r => r.Key != "Content-Type")
                .ForEach(header => listenerResponse.AddHeader(header.Key, header.Value));

            listenerResponse.ContentType = Headers["Content-Type"];
            listenerResponse.StatusCode = StatusCode;
            WriteStream(listenerResponse.OutputStream)
                .Do(stream =>
                        {
                            try
                            {
                                stream.Close(); stream.Dispose();
                            }
                            catch
                            {
                            }

                        }, error => { try { listenerResponse.StatusCode = 500; listenerResponse.OutputStream.Close(); } catch { } })
                .Retry()
                .Subscribe();
        }
    }
    public class HtmlTextResult : Result
    {
        public HtmlTextResult(int statusCode = 200)
            : base(statusCode)
        {
            Headers.Add("Content-Type", "text/html");
        }

    }
    public class EmptyHtmlTextResult : HtmlTextResult
    {
        public EmptyHtmlTextResult(int statusCode = 204)
        {
            StatusCode = statusCode;
        }
    }
    public class BinaryHtmlTextResult : HtmlTextResult
    {
        private readonly byte[] _binary;

        public BinaryHtmlTextResult(byte[] binary, int statusCode = 200)
            : base(statusCode)
        {
            _binary = binary;
        }

        protected override IObservable<Stream> WriteStream(Stream stream)
        {
            stream.Write(_binary, 0, _binary.Length);
            return Observable.Return(stream);
        }
    }
    public class StringHtmlTextResult : BinaryHtmlTextResult
    {
        public StringHtmlTextResult(string message, int statusCode = 200)
            : base(Encoding.UTF8.GetBytes(message), statusCode)
        {
        }
    }
    public class StaticFileHtmlTextResult : HtmlTextResult
    {
        private readonly string _file;

        public StaticFileHtmlTextResult(string file)
        {
            _file = file;
        }

        protected override IObservable<Stream> WriteStream(Stream stream)
        {
            var data = File.ReadAllBytes(_file);
            try
            {
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                stream.Write(new byte[0], 0, 0);
            }

            return Observable.Return(stream);
        }
    }
}