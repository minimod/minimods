using System;

namespace Minimod.EventSourcing.Sample
{
    public class PersonNameChanged
    {
        public readonly Guid Id;
        public readonly string Name;
        public PersonNameChanged(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}