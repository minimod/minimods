using System;
using System.Diagnostics.Contracts;

namespace Minimod.EventSourcing.Sample
{
    public class Person : IAggregateRoot
    {
        private readonly EventSource _eventSource;
        public EventSource EventSource
        {
            get { return _eventSource; }
        }

        private Guid _id;
        private string _name = String.Empty;

        public Person()
        {
            _eventSource  = new EventSource(this);
        }

        public Person(Guid id) : this()
        {
            _id = id;
        }

        public void Create(Guid id, string name)
        {
            Contract.Requires(!id.Equals(Guid.Empty), "Parameter 'id' should not be empty.");
            Contract.Requires(!string.IsNullOrEmpty(name), "Parameter 'name' should not be null or empty");
            _eventSource.Apply(new PersonCreated(id, name));
        }

        public void ChangeName(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name), "Parameter 'name' should not be null or empty");
            _eventSource.Apply(new PersonNameChanged(_id, name));
        }


        public void Handle(PersonNameChanged message)
        {
            _name += message.Name;
        }

        public void Handle(PersonCreated message)
        {
            _id = message.Id;
            _name = message.Name;
        }
    }
}