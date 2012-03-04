using System;
using System.Reactive.Linq;

namespace Minimod.HttpMessageStream.Sample
{
    public class MyLocalHostHttpHttpMessageProcessor : HttpMessageProcessor
    {
        public MyLocalHostHttpHttpMessageProcessor()
            : base(new HttpMessageStream("http://127.0.0.1:1234/"))
        {
            OnReceive<HttpGet>("/1", request => request.Do(ctx => ctx.Result(HelloWorld1)));
            OnReceive<HttpGet>("/2", request => request.Do(ctx => ctx.Result(() => new StringHtmlTextResult("Hello World 2"))));
            OnReceive<HttpGet>("/loadimage/{filename}", request => request.Do(ctx => ctx.Result(() => LoadImage(ctx.Request.UriArguments.filename))));
            OnReceive<HttpPost>("/send", request => request.Do(ctx => ctx.Result(() => new StringHtmlTextResult(ctx.Request.GetBody()))));
        }

        private static Result LoadImage(string filename)
        {
            var result = new StaticFileHtmlTextResult("minimods/sample/images/" + filename);
            result.Headers["Content-Type"] = "image/jpeg";
            return result;
        }

        public static Result HelloWorld1()
        {
            return new StringHtmlTextResult("Hello World 1");
        }
    }

    class HttpServer
    {
        static void Main(string[] args)
        {
            using (new MyLocalHostHttpHttpMessageProcessor())
                Console.ReadLine();
        }
    }
}