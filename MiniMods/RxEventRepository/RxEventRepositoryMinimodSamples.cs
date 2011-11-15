using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Minimod.RxMessageBroker;

namespace Minimod.RxEventRepository
{
    public class RxEventSourcingMinimodeTests
    {
        public void Sample()
        {
            var store = new List<Notification<object>>();
            var eventStore = new RxEventRepositoryMinimod(TimeSpan.FromSeconds(0), 1);
            eventStore.Store<MessageTypeTwo>(RxMessageBrokerMinimod.Default.Stream, store.Add);
            eventStore.Store<MessageTypeOne>(RxMessageBrokerMinimod.Default.Stream, store.Add);

            Observable.Interval(TimeSpan.FromSeconds(15)).Subscribe(x =>
                                                                        {
                                                                            var data1 = eventStore.Restore<MessageTypeTwo>(store);
                                                                            var data2 = eventStore.Restore<MessageTypeOne>(store);
                                                                        });
        }
    }
    public class MessageTypeOne
    {
    }

    public class MessageTypeTwo
    {
    }
}