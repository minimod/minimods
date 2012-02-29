using System.IO;
using System.Net;
using System.Text;

namespace Minimod.HttpMessageStream.Utils
{
    public class WebRequest
    {
        public static HttpWebResponse ExecuteHttpGet(string url)
        {
            var request = System.Net.WebRequest.Create(url);
            request.Method = "GET";
            request.ContentLength = 0;
            return (HttpWebResponse)request.GetResponse();
        }
        public static HttpWebResponse ExecuteHttpPost(string url, string contentType = "text/plain;charset=UTF-8", string data = null)
        {
            var request = System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            if (data == null)
            {
                request.ContentLength = 0;
            }
            else
            {
                using (var requestStream = request.GetRequestStream())
                using (var writer = new StreamWriter(requestStream, new UTF8Encoding(false)))
                    writer.Write(data);                
            }
            return (HttpWebResponse)request.GetResponse();
        }
        public static HttpWebResponse ExecuteHttpPut(string url)
        {
            var request = System.Net.WebRequest.Create(url);
            request.Method = "PUT";
            return (HttpWebResponse)request.GetResponse();
        }
        public static HttpWebResponse ExecuteHttpDelete(string url)
        {
            var request = System.Net.WebRequest.Create(url);
            request.Method = "DELETE";
            return (HttpWebResponse)request.GetResponse();
        }
    }
    public static class WebResponseHelpers
    {
        public static string ReadAllContent(this WebResponse response)
        {
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                return streamReader.ReadToEnd();
        }
        public static T ToType<T>(this object o)
        {
            return (T)o;
        }
    }
}