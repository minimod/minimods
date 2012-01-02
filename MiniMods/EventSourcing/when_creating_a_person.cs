using System;
using Machine.Specifications;
using Minimod.EventSourcing.Sample;

namespace Minimod.EventSourcing
{
    public class when_creating_a_person_with_valid_person_id_and_name
    {
        private static Person _person;
        private static Guid _expectedPersonId;
        private static string _expectedName = "Max Maier";
        private static Exception _exception;

        Because of = () =>
                         {
                             _expectedPersonId = Guid.NewGuid();
                             _person = new Person();
                             _exception = Catch.Exception(() => _person.Create(_expectedPersonId, _expectedName));
                             _person.ChangeName(" Super");
                         };
        
        private It should_apply_person_created_event_with_expected_name = () => _person.ShouldHaveEvent<PersonCreated>(person => person.Name.ShouldEqual(_expectedName));
        private It should_apply_person_created_event_with_expected_person_id = () => _person.ShouldHaveEvent<PersonCreated>(person => person.Id.ShouldEqual(_expectedPersonId));
        private It should_throw_a_exception_when_not_given_a_valid_name_or_person_id = () => _exception.ShouldBeNull();
        private It should_apply_person_name_changed_with_new_name = () => _person.ShouldHaveEvent<PersonNameChanged>(person => person.Name.ShouldEndWith(" Super"));
    }
}