using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Minimod.HttpMessageStream.Utils;

namespace Minimod.HttpMessageStream.Sample
{
    class HttpClient
    {
        static void Main(string[] args)
        {
            Enumerable.Range(1, 10000000).AsParallel().WithDegreeOfParallelism(4).ForEach(x => Task.Factory.StartNew(() =>
            {
                try
                {
                    WebRequest.ExecuteHttpGet("http://127.0.0.1:1234/loadimage/desert.jpg").ReadAllContent();
                    Console.WriteLine(x + ": OK : " + Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }

            }));
            Console.ReadLine();
        }
    }
}