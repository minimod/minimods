using System;
using System.Linq;
using Machine.Specifications;

namespace Minimod.EventSourcing
{
    public static class MSpecIAggregateRootExtentions
    {
        public static void ShouldHaveEvent<T>(this IAggregateRoot value, Action<T> condition)
        {
            var eventMessages = value.EventSource.GetUncommittedApplied().OfType<T>();
            eventMessages.ShouldNotBeEmpty();
            foreach (var eventMessage in eventMessages)
            {
                condition(eventMessage);
            }                
        }
    }
}