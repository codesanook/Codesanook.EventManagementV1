using Codesanook.EventManagement.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace Codesanook.EventManagement.Mappings {
    public class EventBookingRecordMap : IAutoMappingOverride<EventBookingRecord> {
        public void Override(AutoMapping<EventBookingRecord> mapping) {
            // Many to one
            mapping.References(x => x.User).Column("UserId");
            mapping.References(x => x.Event).Column("EventId");

            // One to many, and insert and delete a child if it has been deleted 
            mapping
                .HasMany(x => x.EventAttendees)
                .Inverse() // Let EventAttend side insert with forign key, prevent unnecessary update
                .Cascade.All() // Also insert a child item when save insert this object
                .KeyColumn("EventBookingId"); // EventAttendeeRecord table

            // To store the name or the value of your enum members. To store it by name, just map like this:
            // mapping.Map(x=>x.Status).CustomType<GenericEnumMapper<EventBookingStatus>>().Not.Nullable();
            // To store by value (int)
            // https://stackoverflow.com/a/2605145/1872200
            mapping.Map(x => x.Status).CustomType<int>().Not.Nullable();
        }
    }
}