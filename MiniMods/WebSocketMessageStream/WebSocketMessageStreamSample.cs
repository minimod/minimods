using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

namespace Minimod.WebSocketMessageStream
{
    /// <summary>
    /// Sample for Minimod.WebSocketMessageStream, Version 0.0.5
    /// <para></para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal class Program
    {
        static void Main(string[] args)
        {
            var socket = new WebSocketMessageStream("ws://locahost:8181");
            socket.Subscribe(x =>
            {
                socket.Send(x);
                Debug.WriteLine(x.ToString());
            });

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