using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Web;
using System.Text;
using Minimod.HttpMessageStream.Utils;

namespace Minimod.HttpMessageStream
{
    public class Request
    {        
        public string HttpMethod { get; set; }
        public IDictionary<string, IEnumerable<string>> Headers { get; set; }
        public Stream InputStream { get; set; }
        public Encoding ContentEncoding { get; set; }
        public string RawUrl { get { return Url.ToString(); } }
        public dynamic UriArguments { get; set; }
        public dynamic QueryString { get; private set; }
        
        public int ContentLength
        {
            get { return int.Parse(Headers["Content-Length"].First()); }
        }
        private Uri _url;
        public Uri Url
        {
            get { return _url; }
            set
            {
                _url = value;
                QueryString = new ArgumentsToDynamic(HttpUtility.ParseQueryString(_url.Query));
            }
        }

        public string GetBody()
        {
            return new StreamReader(InputStream).ReadToEnd();
        }        
        public IObservable<string> GetBody(int maxContentLength = 500000)
        {
            var bufferSize = Math.Min(maxContentLength, ContentLength);            
            var buffer = new byte[bufferSize];
            return Observable.FromAsyncPattern<byte[], int, int, int>(InputStream.BeginRead, InputStream.EndRead)(buffer, 0, bufferSize)
                .Select(bytesRead => ContentEncoding.GetString(buffer, 0, bytesRead));            
        }

    }
}