using System;
using Machine.Specifications;
using Minimod.EventSourcing.Sample;

namespace Minimod.EventSourcing
{
    public class a_created_person
    {
        protected static string Name;
        protected static Guid PersonId;

        private Establish context = () =>
        {
            PersonId = Guid.NewGuid();
            Name = "Max Maier";
            new Person().Create(PersonId, Name);
        };
    }
}