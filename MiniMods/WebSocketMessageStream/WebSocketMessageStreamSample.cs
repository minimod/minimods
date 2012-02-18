using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

namespace Minimod.WebSocketMessageStream
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var socket = new WebSocketMessageStream("locahost:8181");
            socket.Subscribe(x => Debug.WriteLine(x.ToString()));

            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                .Repeat()
                .ToObservable();

            Observable
                .Interval(TimeSpan.FromMilliseconds(1000))
                .Zip(alphabet, (time, letter) => new { time, letter })
                .Select(value => value.letter.ToString(CultureInfo.InvariantCulture))
                .Do(socket.Send)
                .Subscribe();
            Console.ReadLine();
        }
    }
}