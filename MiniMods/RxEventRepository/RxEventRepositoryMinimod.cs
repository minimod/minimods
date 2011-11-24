using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Minimod.RxEventRepository
{
    /// <summary>
    /// Minimod.RxEventRepository, Version 0.0.1
    /// <para>A minimod for persisting event messages using Rx.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class RxEventRepositoryMinimod
    {
        private readonly TimeSpan _bufferTime;
        private readonly int _bufferCount;

        public RxEventRepositoryMinimod()
            : this(TimeSpan.FromSeconds(10), 100)
        {

        }

        public RxEventRepositoryMinimod(TimeSpan bufferTime, int bufferCount)
        {
            _bufferTime = bufferTime;
            _bufferCount = bufferCount;
        }

        public void Store<T>(IObservable<object> stream, Action<Notification<object>> store)
        {
            stream
                .Where(x => x != null && x.GetType() == typeof(T))
                .Materialize()
                .Buffer(_bufferTime, _bufferCount)
                .SelectMany(x => x.Select(y => y))
                .Subscribe(store);
        }
        public IEnumerable<T> Restore<T>(IEnumerable<Notification<object>> store)
        {
            return store
                .ToObservable()
                .Dematerialize()
                .OfType<T>()
                .ToEnumerable();
        }
    }
}