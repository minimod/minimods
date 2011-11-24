using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minimod.EventSourcing
{
    public interface IAggregateRoot
    {
        EventSource EventSource { get; }
    }

    public interface IEventSource
    {        
        IEnumerable<object> GetUncommittedApplied();
        void MarkAppliedAsCommitted();
        void Apply(IEnumerable<object> eventMessages);
        void Apply(object eventMessage);
    }

    public class EventSource : IEventSource
    {
        private readonly IAggregateRoot _aggregateRoot;
        private readonly List<object> _changes = new List<object>();
        private readonly Func<object, MethodInfo> _matchedApplyMethodFunc;

        public EventSource(IAggregateRoot aggregateRoot)
        {
            _aggregateRoot = aggregateRoot;
            _matchedApplyMethodFunc = FunctionalExtentions.Memoize<object, MethodInfo>(FindApply);
        }

        public IEnumerable<object> GetUncommittedApplied()
        {
            return _changes;
        }

        public void MarkAppliedAsCommitted()
        {
            _changes.Clear();
        }

        public void Apply(IEnumerable<object> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
                InvokeApply(eventMessage);
        }

        public void Apply(object eventMessage)
        {
            InvokeApply(eventMessage);
            _changes.Add(eventMessage);
        }

        private MethodInfo FindApply(object eventMessage)
        {
            return (from methodInfo in _aggregateRoot.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    from parameterInfo in methodInfo.GetParameters()
                    where parameterInfo.ParameterType == eventMessage.GetType() && methodInfo.Name.Contains("Handle")
                    select methodInfo).Single();
        }

        private void InvokeApply(object eventMessage)
        {
            var matchedMethod = _matchedApplyMethodFunc(eventMessage);
            if (matchedMethod != null)
                matchedMethod.Invoke(_aggregateRoot, new[] { eventMessage });
        }
    }
}
