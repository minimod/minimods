using System;

namespace Minimod.EventSourcing.Sample
{
    public class PersonCreated
    {
        public readonly Guid Id;
        public readonly string Name;
        public PersonCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}